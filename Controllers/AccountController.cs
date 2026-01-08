using EcommerceStore.Data;
using EcommerceStore.Models;
using EcommerceStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        // =========================
        // LOGIN PAGE
        // =========================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // =========================
        // LOGIN (EMAIL + PASSWORD)
        // =========================
       [HttpPost]
public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
{
    ViewData["ReturnUrl"] = returnUrl;

    var user = await _userManager.FindByEmailAsync(email);
    if (user == null)
    {
        TempData["Error"] = "Invalid email or password";
        return View();
    }

    var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, password, false);
    if (!passwordCheck.Succeeded)
    {
        TempData["Error"] = "Invalid email or password";
        return View();
    }

    // ✅ Directly sign in the user
    await _signInManager.SignInAsync(user, false);

    // Redirect to returnUrl if provided, else to Home/Index
    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        return Redirect(returnUrl);

    return RedirectToAction("Index", "Home");
}


        // =========================
        // LOGOUT
        // =========================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // =========================
        // ACCESS DENIED
        // =========================
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
