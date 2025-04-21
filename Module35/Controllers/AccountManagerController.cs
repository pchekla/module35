using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Module35.Models;
using Module35.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Module35.Extensions;
using Microsoft.Extensions.Logging;

namespace Module35.Controllers;

public class AccountManagerController : Controller
{
    private IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AccountManagerController> _logger;

    public AccountManagerController(UserManager<User> userManager, SignInManager<User> signInManager, IMapper mapper, ILogger<AccountManagerController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _mapper = mapper;
        _logger = logger;
    }

    [Route("Login")]
    [HttpGet]
    public IActionResult Login()
    {
        return View("Home/Login");
    }

    [Route("Login")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = _mapper.Map<User>(model);

            if (string.IsNullOrEmpty(user.UserName))
            {
                TempData["ErrorMessage"] = "Email не может быть пустым";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Проверяем, существует ли пользователь с таким email/логином
                var existingUser = await _userManager.FindByNameAsync(user.UserName);
                if (existingUser == null)
                {
                    TempData["ErrorMessage"] = "Пользователь с таким логином не найден";
                    return RedirectToAction("Index", "Home");
                }

                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("MyPage", "AccountManager");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Неверный пароль";
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                // Если возникает ошибка с колонками, направляем пользователя на миграцию
                TempData["ErrorMessage"] = "Ошибка базы данных: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }
        
        TempData["ErrorMessage"] = "Пожалуйста, заполните все необходимые поля";
        return RedirectToAction("Index", "Home");
    }

    [Route("Logout")]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Route("MyPage")]
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MyPage() 
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user == null)
        {
            TempData["ErrorMessage"] = "Пользователь не найден.";
            return RedirectToAction("Index", "Home");
        }
        
        var model = new UserViewModel(user)
        {
            // Устанавливаем флаг, что это собственный профиль пользователя
            IsOwnProfile = true
        };
        
        // Загружаем список друзей пользователя
        try 
        {
            // Получаем список принятых отношений дружбы
            var friendRelations = _userManager.Users.Where(u => u.Id == user.Id)
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
            _logger.LogError(ex, "Ошибка при загрузке списка друзей: {Message}", ex.Message);
            // Даже в случае ошибки мы продолжаем показывать профиль пользователя
        }
        
        return View("User", model);
    }

    [Route("Edit")]
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user == null)
        {
            TempData["ErrorMessage"] = "Пользователь не найден.";
            return RedirectToAction("Index", "Home");
        }
        
        var model = new UserEditViewModel
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            Email = user.Email,
            BirthDate = user.BirthDate,
            Image = user.Image ?? string.Empty,
            Status = user.Status,
            About = user.About
        };
        
        return View(model);
    }
    
    [Authorize]
    [Route("Update")]
    [HttpPost]
    public async Task<IActionResult> Update(UserEditViewModel model) 
    {
        // Очищаем все ошибки валидации для необязательных полей
        ModelState.Remove("Image");
        ModelState.Remove("Status");
        ModelState.Remove("About");
        
        _logger.LogInformation("Обновление профиля. Image: '{Image}', Status: '{Status}', About: '{About}', ModelState валиден: {IsValid}", 
            model.Image, model.Status, model.About, ModelState.IsValid);
        
        // Проверка URL только если не пустой
        if (!string.IsNullOrWhiteSpace(model.Image) && !Uri.IsWellFormedUriString(model.Image, UriKind.Absolute))
        {
            ModelState.AddModelError("Image", "Введите корректный URL");
            _logger.LogWarning("Некорректный URL изображения: {Image}", model.Image);
            return View("Edit", model);
        }

        if (ModelState.IsValid) 
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            
            if (user == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден.";
                _logger.LogWarning("Пользователь не найден при обновлении профиля. UserId: {UserId}", model.UserId);
                return RedirectToAction("Index", "Home");
            }

            // Сохраняем старый email для проверки изменений
            var oldEmail = user.Email;
            
            // Применяем изменения из модели
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.MiddleName = model.MiddleName;
            user.BirthDate = model.BirthDate;
            user.Image = model.Image ?? string.Empty;
            user.Status = string.IsNullOrWhiteSpace(model.Status) ? "" : model.Status;
            user.About = string.IsNullOrWhiteSpace(model.About) ? "" : model.About;
            
            // Если email изменился, обновляем только Email и NormalizedEmail
            // UserName остается неизменным
            if (oldEmail != model.Email)
            {
                user.Email = model.Email;
                user.NormalizedEmail = _userManager.NormalizeEmail(model.Email);
                // Не изменяем UserName и NormalizedUserName
            }

            _logger.LogInformation("Обновление пользователя. UserId: {UserId}, Image: {Image}", user.Id, user.Image);
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded) 
            {
                _logger.LogInformation("Профиль успешно обновлен. UserId: {UserId}", user.Id);
                
                // Гарантируем, что TempData будет доступно после редиректа
                TempData["SuccessMessage"] = "Профиль успешно обновлен!";
                
                // Сохраняем TempData перед редиректом
                TempData.Keep("SuccessMessage");
                
                return RedirectToAction("MyPage", "AccountManager");
            } 
            else 
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError("Ошибка обновления профиля: {Error}", error.Description);
                    ModelState.AddModelError("", error.Description);
                }
                return View("Edit", model);
            }
        } 
        else 
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            _logger.LogWarning("Модель невалидна. Ошибки: {Errors}", string.Join(", ", errors));
            ModelState.AddModelError("", "Некорректные данные");
            return View("Edit", model);
        }
    }
} 