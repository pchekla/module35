using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Module35.Data;
using Module35.Models;
using Module35.ViewModels;
using System.Text;

namespace Module35.Controllers;

[Authorize]
public class FriendsController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FriendsController> _logger;

    public FriendsController(
        UserManager<User> userManager,
        ApplicationDbContext context,
        ILogger<FriendsController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Route("MyFriends")]
    public async Task<IActionResult> Index(string query)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "AccountManager");
        }

        var model = new FriendsListViewModel
        {
            SearchQuery = query
        };

        try
        {
            // Получение принятых друзей
            var acceptedFriendships = await _context.FriendRelations
                .Include(fr => fr.Friend)
                .Where(fr => fr.UserId == currentUser.Id && fr.IsAccepted)
                .ToListAsync();

            model.Friends = acceptedFriendships
                .Select(fr => new FriendViewModel(fr.Friend, fr))
                .ToList();

            // Получение входящих запросов в друзья
            var pendingRequests = await _context.FriendRelations
                .Include(fr => fr.User)
                .Where(fr => fr.FriendId == currentUser.Id && !fr.IsAccepted)
                .ToListAsync();

            model.PendingRequests = pendingRequests
                .Select(fr => new FriendViewModel(fr.User, fr))
                .ToList();

            // Получение исходящих запросов в друзья
            var outgoingRequests = await _context.FriendRelations
                .Include(fr => fr.Friend)
                .Where(fr => fr.UserId == currentUser.Id && !fr.IsAccepted)
                .ToListAsync();

            model.OutgoingRequests = outgoingRequests
                .Select(fr => new FriendViewModel(fr.Friend, fr))
                .ToList();

            // Поиск пользователей, если есть запрос
            if (!string.IsNullOrWhiteSpace(query))
            {
                try 
                {
                    // Используем сырой SQL запрос, который безопасно обрабатывает NULL значения
                    string rawSql = @"
                        SELECT 
                            Id, 
                            ISNULL(FirstName, '') AS FirstName, 
                            ISNULL(LastName, '') AS LastName, 
                            ISNULL(MiddleName, '') AS MiddleName, 
                            ISNULL(Email, '') AS Email, 
                            ISNULL(Image, '') AS Image, 
                            BirthDate, 
                            ISNULL(Status, '') AS Status, 
                            ISNULL(About, '') AS About, 
                            ISNULL(UserName, '') AS UserName 
                        FROM AspNetUsers 
                        WHERE Id <> {0} AND (
                            (FirstName IS NOT NULL AND FirstName LIKE {1}) OR
                            (LastName IS NOT NULL AND LastName LIKE {1}) OR
                            (Email IS NOT NULL AND Email LIKE {1}) OR
                            (UserName IS NOT NULL AND UserName LIKE {1})
                        )";

                    // Экранируем запрос для использования в LIKE
                    string searchPattern = "%" + query + "%";

                    // Выполняем SQL запрос напрямую
                    var userDtos = await _context.UserDtos
                        .FromSqlRaw(rawSql, currentUser.Id, searchPattern)
                        .ToListAsync();

                    // Преобразуем DTO в объекты User
                    var searchResults = userDtos.Select(dto => new User
                    {
                        Id = dto.Id,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        MiddleName = dto.MiddleName,
                        Email = dto.Email,
                        Image = dto.Image,
                        BirthDate = dto.BirthDate,
                        Status = dto.Status,
                        About = dto.About,
                        UserName = dto.UserName
                    }).ToList();

                    // Получаем существующие отношения с найденными пользователями
                    var userIds = searchResults.Select(u => u.Id).ToList();
                    var existingRelations = new List<FriendRelation>();
                    
                    if (userIds.Any())
                    {
                        existingRelations = await _context.FriendRelations
                            .Where(fr => 
                                (fr.UserId == currentUser.Id && userIds.Contains(fr.FriendId)) || 
                                (fr.FriendId == currentUser.Id && userIds.Contains(fr.UserId)))
                            .ToListAsync();
                    }

                    model.SearchResults = searchResults
                        .Select(user => {
                            // Проверяем отношения с текущим пользователем
                            var relation = existingRelations.FirstOrDefault(r => 
                                (r.UserId == currentUser.Id && r.FriendId == user.Id) || 
                                (r.FriendId == currentUser.Id && r.UserId == user.Id));
                            
                            return new FriendViewModel(user, relation);
                        })
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при поиске пользователей: {Message}", ex.Message);
                    TempData["ErrorMessage"] = "Произошла ошибка при поиске пользователей: " + ex.Message;
                    model.SearchResults = new List<FriendViewModel>();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке данных о друзьях: {Message}", ex.Message);
            TempData["ErrorMessage"] = "Произошла ошибка при загрузке данных: " + ex.Message;
        }

        return View(model);
    }

    [HttpGet]
    [Route("Search")]
    public IActionResult SearchRedirect(string query)
    {
        return RedirectToAction("Index", new { query });
    }

    [HttpGet]
    [Route("UserProfile/{userId}")]
    public async Task<IActionResult> ViewProfile(string userId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "AccountManager");
        }

        var userToView = await _userManager.FindByIdAsync(userId);
        if (userToView == null)
        {
            TempData["ErrorMessage"] = "Пользователь не найден.";
            return RedirectToAction("Index", "Home");
        }

        if (userToView.Id == currentUser.Id)
        {
            return RedirectToAction("MyPage", "AccountManager");
        }

        // Проверяем существующие отношения между пользователями
        var relation = await _context.FriendRelations
            .FirstOrDefaultAsync(fr => 
                (fr.UserId == currentUser.Id && fr.FriendId == userToView.Id) || 
                (fr.UserId == userToView.Id && fr.FriendId == currentUser.Id));

        var model = new UserViewModel(userToView)
        {
            IsFriend = relation != null && relation.IsAccepted,
            IsPendingRequest = relation != null && !relation.IsAccepted,
            FriendRequestSentByMe = relation != null && !relation.IsAccepted && relation.UserId == currentUser.Id,
            RelationId = relation?.Id,
            IsOwnProfile = userToView.Id == currentUser.Id
        };

        // Загружаем список друзей просматриваемого пользователя
        try 
        {
            // Получаем список принятых отношений дружбы для просматриваемого пользователя
            var friendRelations = _context.Users
                .Where(u => u.Id == userToView.Id)
                .SelectMany(u => u.SentFriendRequests.Where(fr => fr.IsAccepted)
                    .Select(fr => new { Friend = fr.Friend, Relation = fr })
                    .Union(u.ReceivedFriendRequests.Where(fr => fr.IsAccepted)
                        .Select(fr => new { Friend = fr.User, Relation = fr })))
                .ToList();
                
            if (friendRelations.Any())
            {
                model.Friends = friendRelations
                    .Select(fr => new FriendViewModel(fr.Friend, fr.Relation))
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке списка друзей для пользователя {UserId}: {Message}", 
                userId, ex.Message);
            // Даже в случае ошибки мы продолжаем показывать профиль пользователя
        }

        return View("../AccountManager/User", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendFriendRequest(string userId)
    {
        try 
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "AccountManager");
            }

            // Проверка, что пользователь не пытается добавить себя в друзья
            if (currentUser.Id == userId)
            {
                TempData["ErrorMessage"] = "Вы не можете добавить себя в друзья.";
                return RedirectToAction("Index");
            }

            // Безопасное получение пользователя по Id напрямую из базы
            var userToAdd = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
                
            if (userToAdd == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден.";
                return RedirectToAction("Index");
            }

            // Проверяем, существует ли уже запрос в друзья
            var existingRelation = await _context.FriendRelations
                .FirstOrDefaultAsync(fr => 
                    (fr.UserId == currentUser.Id && fr.FriendId == userId) || 
                    (fr.UserId == userId && fr.FriendId == currentUser.Id));

            if (existingRelation != null)
            {
                TempData["InfoMessage"] = "Запрос в друзья уже отправлен или вы уже друзья.";
                return RedirectToAction("Index");
            }

            // Создаем новый запрос в друзья
            var friendRequest = new FriendRelation
            {
                UserId = currentUser.Id,
                FriendId = userId,
                RequestDate = DateTime.Now,
                IsAccepted = false
            };

            await _context.FriendRelations.AddAsync(friendRequest);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Запрос в друзья отправлен.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке запроса в друзья: {Message}", ex.Message);
            TempData["ErrorMessage"] = "Произошла ошибка при отправке запроса в друзья: " + ex.Message;
        }
        
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptFriendRequest(int relationId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "AccountManager");
        }

        var relation = await _context.FriendRelations
            .FirstOrDefaultAsync(fr => fr.Id == relationId && fr.FriendId == currentUser.Id);

        if (relation == null)
        {
            TempData["ErrorMessage"] = "Запрос в друзья не найден.";
            return RedirectToAction("Index");
        }

        relation.IsAccepted = true;
        relation.AcceptedDate = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Запрос в друзья принят.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeclineFriendRequest(int relationId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "AccountManager");
        }

        var relation = await _context.FriendRelations
            .FirstOrDefaultAsync(fr => fr.Id == relationId && fr.FriendId == currentUser.Id);

        if (relation == null)
        {
            TempData["ErrorMessage"] = "Запрос в друзья не найден.";
            return RedirectToAction("Index");
        }

        _context.FriendRelations.Remove(relation);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Запрос в друзья отклонен.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelFriendRequest(int relationId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "AccountManager");
        }

        var relation = await _context.FriendRelations
            .FirstOrDefaultAsync(fr => fr.Id == relationId && fr.UserId == currentUser.Id && !fr.IsAccepted);

        if (relation == null)
        {
            TempData["ErrorMessage"] = "Запрос в друзья не найден.";
            return RedirectToAction("Index");
        }

        _context.FriendRelations.Remove(relation);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Запрос в друзья отменен.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFriend(int relationId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToAction("Login", "AccountManager");
        }

        var relation = await _context.FriendRelations
            .FirstOrDefaultAsync(fr => fr.Id == relationId && 
                ((fr.UserId == currentUser.Id) || (fr.FriendId == currentUser.Id)));

        if (relation == null)
        {
            TempData["ErrorMessage"] = "Друг не найден.";
            return RedirectToAction("Index");
        }

        _context.FriendRelations.Remove(relation);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Друг удален из списка друзей.";
        return RedirectToAction("Index");
    }
} 