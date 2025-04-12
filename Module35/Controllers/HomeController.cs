using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Module35.Models;

namespace Module35.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Если пользователь авторизован, перенаправляем на страницу для авторизованных пользователей
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Main");
        }
        
        return View();
    }
    
    [Authorize]
    public IActionResult Main()
    {
        // Это страница для авторизованных пользователей
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}