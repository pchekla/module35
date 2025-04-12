using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Module35.Models;
using Module35.ViewModels;

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
                    return RedirectToAction("Main", "Home");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Неверный пароль";
                return RedirectToAction("Index", "Home");
            }
        }
        
        TempData["ErrorMessage"] = "Пожалуйста, заполните все необходимые поля";
        return RedirectToAction("Index", "Home");
    }

    [Route("Logout")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
} 