using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models;
using MyMvcApp.Models.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using MyMvcApp.Models.Interfaces;

namespace MyMvcApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IAuditService _auditService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSender emailSender, IAuditService auditService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _auditService = auditService;
        }

        // Только для администратора
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model?.Email) || string.IsNullOrEmpty(model?.Password))
                {
                    ModelState.AddModelError("", "Email и пароль обязательны.");
                    return View(model);
                }

                var user = new AppUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var roleToAssign = model.SelectedRole == "Admin" ? "Admin" : "User";
                    await _userManager.AddToRoleAsync(user, roleToAssign);
                    TempData["SuccessMessage"] = $"Пользователь {model.Email} успешно создан.";
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        // Вход доступен всем (неавторизованным)
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Неверный email или пароль.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                await _auditService.LogAsync("Login", "Account", null, $"Пользователь {model.Email} вошёл в систему");
                return RedirectToLocal(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                // Отправляем код на почту
                var code = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
                await _emailSender.SendEmailAsync(user.Email, "Код подтверждения", $"Ваш код: {code}");
                return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
            }
            ModelState.AddModelError("", "Неверный email или пароль.");
            return View(model);
        }

        [HttpGet]
        public IActionResult LoginWith2fa(string? returnUrl = null, bool rememberMe = false)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["RememberMe"] = rememberMe;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(string code, string? returnUrl = null, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(code))
            {
                ModelState.AddModelError("", "Код обязателен.");
                return View();
            }

            var result = await _signInManager.TwoFactorSignInAsync(TokenOptions.DefaultEmailProvider, code, rememberMe, rememberClient: false);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Учётная запись заблокирована.");
                return View();
            }
            ModelState.AddModelError("", "Неверный код подтверждения.");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await _auditService.LogAsync("Logout", "Account", null, $"Пользователь {User.Identity?.Name} вышел");
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                Email = user.Email ?? "не указан",
                EmailConfirmed = user.EmailConfirmed,
                UserName = user.UserName ?? "пользователь",
                TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user)
            };
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRole = roles.FirstOrDefault() ?? "User";

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword() => View();

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Пароль успешно изменён.";
                return RedirectToAction("Profile");
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Enable2FA()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            await _userManager.SetTwoFactorEnabledAsync(user, true);
            TempData["SuccessMessage"] = "Двухфакторная аутентификация включена.";
            return RedirectToAction("Profile");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Disable2FA()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            await _userManager.SetTwoFactorEnabledAsync(user, false);
            TempData["SuccessMessage"] = "Двухфакторная аутентификация отключена.";
            return RedirectToAction("Profile");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (User.IsInRole("Admin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count == 1)
                {
                    TempData["ErrorMessage"] = "Нельзя удалить единственного администратора.";
                    return RedirectToAction("Profile");
                }
            }

            await _auditService.LogAsync("DeleteAccount", "Account", null, $"Пользователь {user.Email} удалил свой аккаунт");

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                TempData["SuccessMessage"] = "Ваш аккаунт был удалён.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            TempData["ErrorMessage"] = "Не удалось удалить аккаунт. Попробуйте позже.";
            return RedirectToAction("Profile");
        }
    }
}