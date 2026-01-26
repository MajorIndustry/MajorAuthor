// Project: MajorAuthor.Web
// File: Controllers/AccountController.cs
using AspNet.Security.OAuth.Yandex;
using MajorAuthor.Data;
using MajorAuthor.Models;
using MajorAuthor.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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

        // --- HTTP методы ---

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
                values: new { userId = (string)null, code = (string)null, returnUrl = model.ReturnUrl },
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

        [HttpGet]
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
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (remoteError != null)
            {
                return RedirectToAction(nameof(Login), new
                {
                    returnUrl,
                    error = remoteError
                });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login), new
                {
                    returnUrl,
                    error = "Ошибка загрузки информации внешнего входа."
                });
            }

            var result = await _accountService.HandleExternalLoginCallbackAsync(info, returnUrl);

            if (result.Success)
            {
                return LocalRedirect(result.RedirectUrl);
            }

            if (result._Lockout)
            {
                return RedirectToPage("./Lockout");
            }

            if (result._EmailRequired)
            {
                var tempDataKey = $"ExternalLogin_{System.Guid.NewGuid()}";
                TempData[tempDataKey] = JsonSerializer.Serialize(result.TempData);

                return RedirectToAction(nameof(Login), new
                {
                    status = "email_missing",
                    provider = info.LoginProvider,
                    tempDataKey = tempDataKey,
                    returnUrl = returnUrl
                });
            }

            return RedirectToAction(nameof(Login), new
            {
                returnUrl,
                error = result._Error ?? "Ошибка при обработке внешнего входа."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteExternalLoginRegistration([FromBody] ExternalLoginConfirmationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new ExternalLoginJsonResult { Status = "error", Errors = errors });
            }

            if (string.IsNullOrEmpty(model.TempDataKey))
            {
                return Json(new ExternalLoginJsonResult
                {
                    Status = "session_expired",
                    Message = "Сессия внешнего входа истекла. Пожалуйста, попробуйте войти снова."
                });
            }

            var tempDataJson = TempData[model.TempDataKey] as string;
            if (string.IsNullOrEmpty(tempDataJson))
            {
                return Json(new ExternalLoginJsonResult
                {
                    Status = "session_expired",
                    Message = "Сессия внешнего входа истекла. Пожалуйста, попробуйте войти снова."
                });
            }

            var tempData = JsonSerializer.Deserialize<ExternalLoginTempData>(tempDataJson);
            if (tempData == null)
            {
                return Json(new ExternalLoginJsonResult
                {
                    Status = "session_expired",
                    Message = "Сессия внешнего входа истекла. Пожалуйста, попробуйте войти снова."
                });
            }

            var callbackUrl = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                values: new { returnUrl = tempData.ReturnUrl },
                protocol: Request.Scheme);

            var result = await _accountService.CompleteExternalRegistrationAsync(model.Email, tempData, callbackUrl);

            // Обрабатываем случай, когда требуется подтверждение email
            if (result.Status == "email_confirmation_required")
            {
                // Перенаправляем на страницу входа с сообщением
                result.RedirectUrl = Url.Action("Login", "Account");
            }

            return Json(result);
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

            if (result.Succeeded)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    // Автоматически входим после подтверждения email
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Устанавливаем сообщение об успехе
                    TempData["StatusMessage"] = "Спасибо за подтверждение вашего email. Вы успешно вошли в систему.";
                    return LocalRedirect(returnUrl ?? Url.Content("~/"));
                }
            }

            // Если что-то пошло не так
            ViewBag.StatusMessage = "Ошибка подтверждения email.";
            return View("Error");
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
                values: new { userId = (string)null, code = (string)null, returnUrl = Url.Content("~/") },
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
                values: new { email = (string)null, code = (string)null },
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

        [HttpGet]
        private IActionResult ReturnPopupCallback(object responseData)
        {
            var jsonString = System.Text.Json.JsonSerializer.Serialize(responseData);
            ViewData["JsonResponse"] = jsonString;
            return View("ExternalLoginPopupCallback");
        }
    }
}