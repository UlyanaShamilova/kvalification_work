using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using project.Models;
using project.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace project.Controllers;

public class AccController : Controller
{
    private readonly RecipesDbContext _dbContext;
    private readonly PasswordHasher<User> _passwordHasher;

    public AccController(RecipesDbContext dbContext)
    {
        _dbContext = dbContext;
        _passwordHasher = new PasswordHasher<User>();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.username == model.username);
        if (existingUser != null)
        {
            ModelState.AddModelError("", "Пользователь с таким никнеймом уже существует");
            return View(model);
        }

        var user = new User
        {
            username = model.username,
            email = model.email,
        };

        user.password = _passwordHasher.HashPassword(user, model.password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // сохраняем ID в сессию
        HttpContext.Session.SetInt32("UserId", user.userID);

        await SignInUser(user); // создаём куки с данными юзера

        TempData["Message"] = "Ви успішно зареєструвались";
        return RedirectToAction("main_page", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Login(LogModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.username == model.username);
        if (user == null)
        {
            ModelState.AddModelError("", "Неверный никнейм или пароль");
            return View(model);
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.password, model.password);
        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Неверный никнейм или пароль");
            return View(model);
        }

        // сохраняем ID в сессию
        HttpContext.Session.SetInt32("UserId", user.userID);

        await SignInUser(user); // создаём куки с данными юзера

        TempData["Message"] = "Ви успішно авторизувались";
        return RedirectToAction("main_page", "Home");
    }

    [HttpPost]
    public async Task<IActionResult>  Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear(); 
        return RedirectToAction("main_page", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    
     // общий метод для входа
    private async Task SignInUser(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.userID.ToString()),
            new Claim(ClaimTypes.Name, user.username),
            // new Claim("UserId", user.userID.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties
        {
            IsPersistent = false, // куки не будут сохраняться после закрытия браузера
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // куки истекут через 30 минут
        });
    }
}
