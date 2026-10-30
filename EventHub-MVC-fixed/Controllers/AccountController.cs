using System.Security.Claims;
using EventHub.Data;
using EventHub.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Controllers;

public class AccountController : Controller
{
    private readonly UserRepository _users;
    public AccountController(UserRepository users) => _users = users;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginVm());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(vm);

        var user = _users.GetByEmail(vm.Email.Trim());
        if (user == null || !UserRepository.VerifyPassword(vm.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Email or password is incorrect.");
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies"));
        await HttpContext.SignInAsync("Cookies", principal);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterVm());

    [HttpPost]
    public IActionResult Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var email = vm.Email.Trim();
        if (_users.GetByEmail(email) != null)
        {
            ModelState.AddModelError("Email", "This email is already registered.");
            return View(vm);
        }

        // Dang ky cong khai luon la CLIENT. Chi ADMIN moi doi duoc role.
        _users.Create(new User
        {
            Name = vm.Name.Trim(), Email = email,
            PasswordHash = UserRepository.HashPassword(vm.Password), Role = Roles.Client
        });

        TempData["Success"] = "Registration successful. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();
}
