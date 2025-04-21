using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Module35.Data;
using Module35.Models;
using Module35.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Module35.Hubs;

namespace Module35.Controllers;

[Authorize]
public class MessagesController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MessagesController> _logger;
    private readonly IHubContext<ChatHub> _hubContext;
    
    // Статический словарь для отслеживания состояния вода пользователей
    private static readonly Dictionary<string, Dictionary<string, bool>> _usersTypingStatus = 
        new Dictionary<string, Dictionary<string, bool>>();
    
    // Статический словарь для отслеживания последней активности пользователей
    private static readonly Dictionary<string, DateTime> _lastActivity = 
        new Dictionary<string, DateTime>();

    // Временный словарь для хранения состояний печати пользователей
    private static Dictionary<string, Dictionary<string, DateTime>> _typingUsers = 
        new Dictionary<string, Dictionary<string, DateTime>>();

    public MessagesController(
        UserManager<User> userManager,
        ApplicationDbContext context,
        ILogger<MessagesController> logger,
        IHubContext<ChatHub> hubContext)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
    }

    [HttpGet]
    [Route("Chat/{friendId}")]
    public async Task<IActionResult> Chat(string friendId, bool partial = false)
    {
        try
        {
            // Получаем текущего пользователя
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("Попытка доступа к чату неавторизованным пользователем");
                return RedirectToAction("Login", "AccountManager");
            }

            // Проверяем существование пользователя
            var friend = await _userManager.FindByIdAsync(friendId);
            if (friend == null)
            {
                _logger.LogWarning($"Chat attempted with non-existent user ID: {friendId}");
                return NotFound("Пользователь не найден");
            }

            // Проверяем, являются ли пользователи друзьями
            var areFriends = await _context.FriendRelations
                .AnyAsync(fr => 
                    ((fr.UserId == currentUser.Id && fr.FriendId == friendId) ||
                    (fr.UserId == friendId && fr.FriendId == currentUser.Id)) && 
                    fr.IsAccepted);

            if (!areFriends)
            {
                _logger.LogWarning($"Chat attempted between non-friends: {currentUser.Id} and {friendId}");
                return BadRequest("Вы не можете отправлять сообщения этому пользователю, так как он не в вашем списке друзей");
            }

            // Генерируем модель чата
            var chatModel = await GenerateChatModel(currentUser, friend);
            
            // Проверяем, печатает ли друг
            bool isTyping = IsUserTyping(friendId, currentUser.Id);

            // Если это частичное обновление для AJAX, возвращаем HTML и количество сообщений
            if (partial)
            {
                // Рендерим только сообщения
                var messagesHtml = "";
                if (chatModel.Messages.Any())
                {
                    foreach (var message in chatModel.Messages)
                    {
                        messagesHtml += $@"
                            <div class=""mb-3 {(message.IsFromCurrentUser ? "text-end" : "")}"">"
                            + $@"<div class=""d-inline-block message-bubble {(message.IsFromCurrentUser ? "bg-primary text-white" : "bg-light")}"">"
                            + $@"<div class=""message-content"">{message.Text}</div>"
                            + $@"<div class=""message-time {(message.IsFromCurrentUser ? "text-white-50" : "text-muted")}"">{message.SentDate.ToString("HH:mm, dd MMM")}</div>"
                            + "</div></div>";
                    }
                }
                else
                {
                    messagesHtml = $@"
                        <div class=""text-center text-muted my-5"">
                            <i class=""bi bi-chat-dots display-4""></i>
                            <p class=""mt-3"">У вас пока нет сообщений с {chatModel.Friend.GetFullName()}</p>
                            <p>Начните общение прямо сейчас!</p>
                        </div>";
                }

                return Json(new { 
                    success = true, 
                    html = messagesHtml, 
                    messageCount = chatModel.Messages.Count,
                    isTyping = isTyping
                });
            }

            // Обновляем время активности пользователя
            UpdateUserActivity(currentUser.Id);

            return View(chatModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in Chat action with friendId: {friendId}");
            
            if (partial)
            {
                return Json(new { success = false, error = "Произошла ошибка при загрузке сообщений" });
            }
            
            return View("Error", ex.Message);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage(string friendId, MessageViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                
                _logger.LogWarning($"Invalid message model: {errors}");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = errors });
                }
                
                return RedirectToAction(nameof(Chat), new { friendId });
            }

            // Получаем текущего пользователя
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogWarning("Попытка отправки сообщения неавторизованным пользователем");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "Требуется авторизация" });
                }
                
                return RedirectToAction("Login", "AccountManager");
            }
            
            // Проверяем существование пользователя
            var friend = await _userManager.FindByIdAsync(friendId);
            if (friend == null)
            {
                _logger.LogWarning($"Message sent to non-existent user ID: {friendId}");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "Пользователь не найден" });
                }
                
                TempData["ErrorMessage"] = "Пользователь не найден";
                return RedirectToAction(nameof(Chat), new { friendId });
            }

            // Проверяем, являются ли пользователи друзьями
            var areFriends = await _context.FriendRelations
                .AnyAsync(fr => 
                    ((fr.UserId == currentUser.Id && fr.FriendId == friendId) ||
                    (fr.UserId == friendId && fr.FriendId == currentUser.Id)) && 
                    fr.IsAccepted);

            if (!areFriends)
            {
                _logger.LogWarning($"Message sent between non-friends: {currentUser.Id} and {friendId}");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "Вы не можете отправлять сообщения этому пользователю, так как он не в вашем списке друзей" });
                }
                
                TempData["ErrorMessage"] = "Вы не можете отправлять сообщения этому пользователю, так как он не в вашем списке друзей";
                return RedirectToAction(nameof(Chat), new { friendId });
            }

            // Создаем сообщение
            var message = new Message
            {
                Text = model.Text,
                SentDate = DateTime.Now,
                SenderId = currentUser.Id,
                RecipientId = friendId
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Message saved from user {currentUser.Id} to user {friendId}. ID: {message.Id}");
            
            // --- Отправка через SignalR --- 
            var senderDto = new { 
                Id = currentUser.Id, 
                Name = currentUser.GetFullName(), 
                Image = currentUser.Image 
            };
            
            var messageDto = new { 
                Text = message.Text, 
                SentDate = message.SentDate 
            };

            // Отправляем сообщение получателю, если он онлайн
            await _hubContext.Clients.User(friendId).SendAsync(
                "ReceiveMessage", 
                senderDto.Id, 
                senderDto.Name, 
                senderDto.Image,
                messageDto.Text, 
                messageDto.SentDate);
            
            // Отправляем сообщение обратно отправителю для немедленного отображения
             await _hubContext.Clients.User(currentUser.Id).SendAsync(
                 "ReceiveMessage", 
                 senderDto.Id, 
                 senderDto.Name, 
                 senderDto.Image, 
                 messageDto.Text, 
                 messageDto.SentDate);

            // Отправляем глобальное уведомление получателю
             await _hubContext.Clients.User(friendId).SendAsync(
                 "ReceiveNotification", 
                 senderDto.Name, // Отправляем имя
                 messageDto.Text);
                
            // Уведомляем через хаб, что отправитель перестал печатать
            // await _hubContext.Clients.User(friendId).SendAsync(
            //     "ReceiveTypingNotification", 
            //     currentUser.Id, 
            //     false); // Отправитель перестал печатать (можно оставить или убрать, т.к. NotifyTyping вызывается из JS)

            // Если запрос был AJAX, просто возвращаем успех
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true }); // Сообщение будет добавлено через SignalR
            }
            
            // Если не AJAX, делаем редирект (старое поведение)
            TempData["SuccessMessage"] = "Сообщение отправлено";
            return RedirectToAction(nameof(Chat), new { friendId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending message to friendId: {friendId}");
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, error = "Произошла ошибка при отправке сообщения" });
            }
            
            TempData["ErrorMessage"] = "Произошла ошибка при отправке сообщения";
            return RedirectToAction(nameof(Chat), new { friendId });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> NotifyTyping(string friendId, bool isTyping)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser != null)
        {
            // Уведомляем через хаб
            await _hubContext.Clients.User(friendId).SendAsync("ReceiveTypingNotification", currentUser.Id, isTyping);
        }
        return Json(new { success = true });
    }
    
    [HttpPost]
    public IActionResult NotifyMessageSent(string friendId)
    {
        try
        {
            // Получаем текущего пользователя
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                _logger.LogWarning("NotifyMessageSent: Current user is not authenticated or ID is null");
                return Json(new { success = false });
            }
            
            // Сбрасываем статус ввода
            UpdateTypingStatus(currentUserId, friendId, false);
            
            // Обновляем время активности пользователя
            UpdateUserActivity(currentUserId);
            
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении статуса сообщения");
            return Json(new { success = false });
        }
    }

    private async Task<ChatViewModel> GenerateChatModel(User currentUser, User friend)
    {
        // Получаем все сообщения между пользователями, отсортированные по дате
        var messages = await _context.Messages
            .Where(m => 
                (m.SenderId == currentUser.Id && m.RecipientId == friend.Id) ||
                (m.SenderId == friend.Id && m.RecipientId == currentUser.Id))
            .OrderBy(m => m.SentDate)
            .ToListAsync();

        // Создаем модель сообщений
        var messageViewModels = messages.Select(m => new MessageViewModel
        {
            Id = m.Id,
            Text = m.Text,
            SentDate = m.SentDate,
            IsFromCurrentUser = m.SenderId == currentUser.Id
        }).ToList();

        // Создаем модель чата
        var chatModel = new ChatViewModel
        {
            CurrentUser = currentUser,
            Friend = friend,
            FriendId = friend.Id,
            Messages = messageViewModels,
            NewMessage = new MessageViewModel()
        };

        return chatModel;
    }
    
    // Вспомогательный метод для обновления статуса ввода
    private void UpdateTypingStatus(string userId, string friendId, bool isTyping)
    {
        // Инициализируем словарь для пользователя, если его еще нет
        if (!_usersTypingStatus.ContainsKey(userId))
        {
            _usersTypingStatus[userId] = new Dictionary<string, bool>();
        }
        
        // Обновляем статус ввода для конкретного друга
        _usersTypingStatus[userId][friendId] = isTyping;
    }
    
    // Вспомогательный метод для проверки статуса ввода пользователя
    private bool IsUserTyping(string userId, string friendId)
    {
        // Проверяем, печатает ли пользователь сообщение другу
        if (_usersTypingStatus.ContainsKey(userId) && 
            _usersTypingStatus[userId].ContainsKey(friendId))
        {
            // Проверяем, был ли пользователь активен в последние 30 секунд
            if (IsUserActive(userId))
            {
                return _usersTypingStatus[userId][friendId];
            }
        }
        return false;
    }
    
    // Обновляем время последней активности пользователя
    private void UpdateUserActivity(string userId)
    {
        _lastActivity[userId] = DateTime.Now;
    }
    
    // Проверяем, был ли пользователь активен в последние 30 секунд
    private bool IsUserActive(string userId)
    {
        if (_lastActivity.ContainsKey(userId))
        {
            return (DateTime.Now - _lastActivity[userId]).TotalSeconds < 30;
        }
        return false;
    }

    // Метод для проверки, печатает ли друг сообщение
    [HttpGet]
    public IActionResult IsTyping(int friendId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("IsTyping: Пользователь не аутентифицирован");
                return Json(new { isTyping = false });
            }

            string key = $"{friendId}";
            if (_typingUsers.ContainsKey(key) && 
                _typingUsers[key].TryGetValue(userId, out DateTime lastTyped))
            {
                // Если прошло менее 3 секунд с момента последнего уведомления о наборе текста
                bool isStillTyping = (DateTime.Now - lastTyped).TotalSeconds < 3;
                return Json(new { isTyping = isStillTyping });
            }

            return Json(new { isTyping = false });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке состояния набора текста");
            return Json(new { isTyping = false });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages(int friendId)
    {
        _logger.LogInformation($"Getting messages with friend ID: {friendId}");
        
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            _logger.LogWarning("GetMessages: Current user not found");
            return Json(new { success = false, message = "Пользователь не найден" });
        }

        var friend = await _context.Users.FindAsync(friendId);
        if (friend == null)
        {
            _logger.LogWarning($"GetMessages: Friend with ID {friendId} not found");
            return Json(new { success = false, message = "Друг не найден" });
        }

        // Check if users are friends
        var areFriends = await _context.FriendRelations.AnyAsync(fr => 
            (fr.UserId == currentUser.Id && fr.FriendId == friend.Id && fr.IsAccepted) ||
            (fr.UserId == friend.Id && fr.FriendId == currentUser.Id && fr.IsAccepted));

        if (!areFriends)
        {
            _logger.LogWarning($"GetMessages: Users {currentUser.Id} and {friendId} are not friends");
            return Json(new { success = false, message = "Вы не являетесь друзьями с этим пользователем" });
        }

        var messages = await _context.Messages
            .Where(m => (m.SenderId == currentUser.Id && m.RecipientId == friend.Id) || 
                       (m.SenderId == friend.Id && m.RecipientId == currentUser.Id))
            .OrderBy(m => m.SentDate)
            .ToListAsync();
        
        var viewModel = new ChatViewModel
        {
            CurrentUserId = currentUser.Id,
            FriendId = friend.Id,
            Friend = friend,
            CurrentUser = currentUser,
            Messages = messages.Select(m => new MessageViewModel
            {
                Id = m.Id,
                Text = m.Text,
                SentDate = m.SentDate,
                SenderId = m.SenderId,
                SenderName = m.SenderId == currentUser.Id 
                    ? currentUser.FirstName + " " + currentUser.LastName 
                    : friend.FirstName + " " + friend.LastName,
                SenderImage = m.SenderId == currentUser.Id 
                    ? currentUser.Image 
                    : friend.Image,
                RecipientId = m.RecipientId,
                IsFromCurrentUser = m.SenderId == currentUser.Id
            }).ToList()
        };

        return PartialView("_ChatMessages", viewModel);
    }
} 