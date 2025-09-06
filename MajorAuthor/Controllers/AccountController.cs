// Project: MajorAuthor.Web
// File: Controllers/AccountController.cs
using MajorAuthor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using MajorAuthor.Data;
using Microsoft.AspNetCore.Authentication.Google;
using MajorAuthor.Services;
using AspNet.Security.OAuth.Yandex;

namespace MajorAuthor.Controllers
{
    /// <summary>
    /// Controller for managing user accounts.
    /// It now acts as a thin wrapper around the IAccountService, adhering to SRP.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountService _accountService;

        /// <summary>
        /// Controller constructor.
        /// </summary>
        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IAccountService accountService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            var viewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            if (TempData["StatusMessage"] != null)
            {
                ModelState.AddModelError(string.Empty, TempData["StatusMessage"].ToString());
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            model.ReturnUrl = returnUrl;
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _accountService.LoginWithPasswordAsync(model);

            if (result.Succeeded) return LocalRedirect(returnUrl ?? Url.Content("~/"));
            if (result.RequiresTwoFactor) return RedirectToAction("LoginWith2fa", "Account", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
            if (result.IsLockedOut) return RedirectToAction("Lockout", "Account");

            ModelState.AddModelError(string.Empty, "Неверная попытка входа.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            var viewModel = new RegisterViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            model.ReturnUrl = returnUrl ?? Url.Content("~/");
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var callbackUrl = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                values: new { userId = "REPLACE_USER_ID", code = "REPLACE_CODE", returnUrl = model.ReturnUrl },
                protocol: Request.Scheme);

            var (result, user) = await _accountService.RegisterAndSendConfirmationAsync(model, callbackUrl);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = $"Письмо с подтверждением отправлено на {model.Email}. Пожалуйста, проверьте свой Email (включая папку со спамом).";
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            if (provider == GoogleDefaults.AuthenticationScheme || provider == YandexAuthenticationDefaults.AuthenticationScheme)
            {
                properties.SetParameter("prompt", "select_account");
            }

            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["StatusMessage"] = "Ошибка при загрузке информации о внешнем входе.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            var result = await _accountService.HandleExternalLoginCallbackAsync(info, returnUrl);

            switch (result)
            {
                case ExternalLoginResult.Succeeded:
                    return LocalRedirect(returnUrl ?? Url.Content("~/"));
                case ExternalLoginResult.Lockout:
                    return RedirectToAction("Lockout", "Account");
                case ExternalLoginResult.LoginAlreadyAssociated:
                    TempData["StatusMessage"] = "Этот внешний логин уже привязан к другому аккаунту. Пожалуйста, войдите с привязанным аккаунтом.";
                    return RedirectToAction(nameof(Login), new { returnUrl });
                case ExternalLoginResult.RequiresConfirmation:
                    ViewData["ReturnUrl"] = returnUrl;
                    ViewData["LoginProvider"] = info.LoginProvider;
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
                case ExternalLoginResult.LoginFailed:
                default:
                    TempData["StatusMessage"] = "Произошла ошибка при входе через внешний аккаунт. Попробуйте еще раз.";
                    return RedirectToAction(nameof(Login), new { returnUrl });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["StatusMessage"] = "Ошибка при загрузке информации о внешнем входе во время подтверждения.";
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                return View(model);
            }

            var result = await _accountService.ConfirmExternalLoginAsync(info, model);

            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code, string returnUrl = null)
        {
            var result = await _accountService.ConfirmEmailAsync(userId, code);
            ViewBag.StatusMessage = result.Succeeded ? "Спасибо за подтверждение вашего email." : "Ошибка подтверждения email.";

            if (result.Succeeded)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                }
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }
            else
            {
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View(new ResendEmailConfirmationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var callbackUrl = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                values: new { userId = "REPLACE_USER_ID", code = "REPLACE_CODE", returnUrl = Url.Content("~/") },
                protocol: Request.Scheme);

            var success = await _accountService.ResendEmailConfirmationAsync(model, callbackUrl);

            ViewBag.StatusMessage = "Если ваш Email зарегистрирован, вам будет отправлено письмо для подтверждения. Пожалуйста, проверьте папку со спамом.";
            ViewBag.StatusMessageType = "success";

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var callbackUrl = Url.Action(
                nameof(ResetPassword),
                "Account",
                values: new { email = "REPLACE_EMAIL", code = "REPLACE_CODE" },
                protocol: Request.Scheme);

            var success = await _accountService.ForgotPasswordAsync(model, callbackUrl);

            ViewBag.StatusMessage = "Если ваш Email зарегистрирован и подтвержден, вам будет отправлено письмо для сброса пароля. Пожалуйста, проверьте папку со спамом.";
            ViewBag.StatusMessageType = "success";

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string code = null, string email = null)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Home");
            }
            var model = new ResetPasswordViewModel { Code = code, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _accountService.ResetPasswordAsync(model);

            if (result.Succeeded)
            {
                ViewBag.StatusMessage = "Ваш пароль был успешно сброшен. Теперь вы можете войти.";
                ViewBag.StatusMessageType = "success";
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            ViewBag.StatusMessage = "Ошибка при сбросе пароля.";
            ViewBag.StatusMessageType = "error";
            return View(model);
        }

        [HttpGet]
        public IActionResult LoginWith2fa(string returnUrl = null, bool rememberMe = false)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.RememberMe = rememberMe;
            return View();
        }

        [HttpGet]
        public IActionResult Lockout()
        {
            return View();
        }
    }
}
