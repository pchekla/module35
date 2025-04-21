using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Module35.Models;
using Module35.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Module35.Extensions;

namespace Module35.Controllers;

public class AccountManagerController : Controller
{
    private IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AccountManagerController(UserManager<User> userManager, SignInManager<User> signInManager, IMapper mapper)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _mapper = mapper;
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
        
        return View("User", new UserViewModel(user));
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
            Image = user.Image,
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
        if (ModelState.IsValid) 
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            
            if (user == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден.";
                return RedirectToAction("Index", "Home");
            }

            // Сохраняем старый email для проверки изменений
            var oldEmail = user.Email;
            
            // Применяем изменения из модели
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.MiddleName = model.MiddleName;
            user.BirthDate = model.BirthDate;
            user.Image = model.Image;
            user.Status = model.Status;
            user.About = model.About;
            
            // Если email изменился, обновляем только Email и NormalizedEmail
            // UserName остается неизменным
            if (oldEmail != model.Email)
            {
                user.Email = model.Email;
                user.NormalizedEmail = _userManager.NormalizeEmail(model.Email);
                // Не изменяем UserName и NormalizedUserName
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded) 
            {
                TempData["SuccessMessage"] = "Профиль успешно обновлен!";
                return RedirectToAction("MyPage", "AccountManager");
            } 
            else 
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("Edit", model);
            }
        } 
        else 
        {
            ModelState.AddModelError("", "Некорректные данные");
            return View("Edit", model);
        }
    }
} 