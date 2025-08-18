// Project: MajorAuthor.Web
// File: Controllers/AccountController.cs
using MajorAuthor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication; // For AuthenticationScheme
using System.Security.Claims; // For ClaimTypes
using MajorAuthor.Data; // Added for ApplicationUser
using Microsoft.AspNetCore.Authentication.Google; // Added for GoogleDefaults.AuthenticationScheme
using MajorAuthor.Services; // Добавлено для IEmailSender
using System.Text.Encodings.Web; // Добавлено для HtmlEncoder
// using AspNet.Security.OAuth.Yandex; // Можно закомментировать, если YandexDefaults все равно не находится

namespace MajorAuthor.Controllers
{
    /// <summary>
    /// Controller for managing user accounts (login, logout, external logins, registration).
    /// </summary>
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender; // Добавлено

        /// <summary>
        /// Controller constructor.
        /// </summary>
        /// <param name="signInManager">ASP.NET Core Identity sign-in manager.</param>
        /// <param name="userManager">ASP.NET Core Identity user manager.</param>
        /// <param name="emailSender">Служба отправки электронной почты.</param> // Добавлено
        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender; // Инициализируем
        }

        /// <summary>
        /// Displays the login page.
        /// </summary>
        /// <param name="returnUrl">URL to redirect to after login.</param>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear existing external cookies to ensure the user picks a new provider
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var viewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            return View(viewModel);
        }

        /// <summary>
        /// Handles the login form submission.
        /// </summary>
        /// <param name="model">Login form data model.</param>
        /// <param name="returnUrl">URL to redirect to after login.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            model.ReturnUrl = returnUrl;
            // Ensure ExternalLogins are re-populated on POST for view rendering,
            // always populate them at the very beginning of the POST action.
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Attempt to sign in with password
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // If successful, redirect to ReturnUrl or home page
                    return LocalRedirect(returnUrl ?? Url.Content("~/"));
                }
                if (result.RequiresTwoFactor)
                {
                    // If two-factor authentication is required
                    return RedirectToAction("LoginWith2fa", "Account", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    // If account is locked out
                    return RedirectToAction("Lockout", "Account");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Неверная попытка входа.");
                    // Return view with errors, ExternalLogins and ReturnUrl are already populated
                    return View(model);
                }
            }
            // If model is invalid, return view with errors, ExternalLogins and ReturnUrl are already populated
            return View(model);
        }

        /// <summary>
        /// Displays the registration page.
        /// </summary>
        /// <param name="returnUrl">URL to redirect to after registration.</param>
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

        /// <summary>
        /// Handles the registration form submission.
        /// </summary>
        /// <param name="model">Registration form data model.</param>
        /// <param name="returnUrl">URL to redirect to after registration.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Ensure ExternalLogins are re-populated on POST for view rendering,
            // always populate them at the very beginning of the POST action.
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // === ЛОГИКА ОТПРАВКИ ПОДТВЕРЖДЕНИЯ EMAIL ===
                    // Генерируем токен подтверждения email
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Создаем URL обратного вызова для подтверждения email
                    var callbackUrl = Url.Action(
                        "ConfirmEmail", // Имя Action-метода для подтверждения
                        "Account",      // Имя контроллера
                        values: new { userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme); // Используем текущий протокол (http/https)

                    // Отправляем письмо с подтверждением
                    await _emailSender.SendEmailAsync(
                        model.Email,
                        "Подтвердите ваш Email на MajorAuthor",
                        $"Пожалуйста, подтвердите ваш аккаунт, перейдя по ссылке: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Подтвердить Email</a>.");
                    // ===========================================

                    // Если требование подтверждения аккаунта включено (options.SignIn.RequireConfirmedAccount = true)
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        // Перенаправляем на страницу подтверждения регистрации, но не входим пользователя сразу
                        return RedirectToAction("RegisterConfirmation", "Account", new { email = model.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        // Если подтверждение не требуется, сразу входим пользователя
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            // If model is invalid, return view with errors, ExternalLogins and ReturnUrl are already populated
            return View(model);
        }


        /// <summary>
        /// Initiates external login via the specified provider.
        /// </summary>
        /// <param name="provider">Provider name (e.g., "Google", "Yandex").</param>
        /// <param name="returnUrl">URL to redirect to after login.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request redirect to external provider
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // Special handling for Google to force account selection
            if (provider == GoogleDefaults.AuthenticationScheme)
            {
                // To force Google to always show the account selection dialog
                // or if you want to explicitly add any other parameters.
                properties.SetParameter("prompt", "select_account");
            }
            // Добавлено для Яндекса, чтобы принудительно показывать выбор аккаунта
            else if (provider == "Yandex") // Используем строковый литерал "Yandex"
            {
                // Параметр 'prompt=select_account' обычно работает и для Яндекса
                properties.SetParameter("prompt", "select_account");
            }

            return new ChallengeResult(provider, properties);
        }

        /// <summary>
        /// Handles the callback from an external authentication provider.
        /// </summary>
        /// <param name="returnUrl">URL to redirect to after login.</param>
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["ErrorMessage"] = "Ошибка при загрузке информации о внешнем входе.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            // 1. Attempt to sign in if the external login is already associated with an existing user.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                // Successful sign-in via external provider (login already linked).
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction("Lockout", "Account");
            }
            // Если вход не удался (например, NotAllowed, RequiresTwoFactor, Failed)
            else
            {
                // Get Email from external provider's claims (if available).
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                // 2. Check if a user with this Email already exists locally (but external login is not linked).
                if (email != null)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user != null)
                    {
                        // A user with this Email already exists locally.
                        // We need to link the external login to this existing account.
                        var addLoginResult = await _userManager.AddLoginAsync(user, info);
                        if (addLoginResult.Succeeded)
                        {
                            // Успешно привязали внешний логин к существующему аккаунту.
                            // Так как email подтвержден внешним провайдером, помечаем его как подтвержденный в нашей системе.
                            if (!await _userManager.IsEmailConfirmedAsync(user))
                            {
                                user.EmailConfirmed = true;
                                await _userManager.UpdateAsync(user);
                            }
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl ?? Url.Content("~/"));
                        }
                        else
                        {
                            // Error linking external login.
                            foreach (var error in addLoginResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                                if (error.Code == "LoginAlreadyAssociated")
                                {
                                    ModelState.AddModelError(string.Empty, $"Внешний логин уже привязан к другому аккаунту. Пожалуйста, войдите с привязанным аккаунтом.");
                                }
                            }
                            TempData["ErrorMessage"] = "Произошла ошибка при входе через внешний аккаунт. Попробуйте еще раз.";
                            return RedirectToAction(nameof(Login), new { returnUrl });
                        }
                    }
                }

                // If external login not linked AND user with this email not found locally,
                // redirect to ExternalLoginConfirmation for new registration/linking.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
            }
        }

        /// <summary>
        /// Displays the external login confirmation page (for new users).
        /// </summary>
        [HttpGet]
        public IActionResult ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model)
        {
            return View(model);
        }

        /// <summary>
        /// Handles external login confirmation and new user registration.
        /// </summary>
        /// <param name="model">Confirmation model.</param>
        /// <param name="returnUrl">URL to redirect to.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["ErrorMessage"] = "Ошибка при загрузке информации о внешнем входе во время подтверждения.";
                return RedirectToAction(nameof(Login));
            }

            if (ModelState.IsValid)
            {
                var email = model.Email;
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    // User with this email not found, create a new one.
                    user = new ApplicationUser { UserName = email, Email = email };
                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        // Пользователь успешно создан.
                        // Так как email подтвержден внешним провайдером, помечаем его как подтвержденный в нашей системе.
                        user.EmailConfirmed = true;
                        await _userManager.UpdateAsync(user); // Сохраняем изменение EmailConfirmed

                        // Теперь привязываем внешний логин
                        var addLoginResult = await _userManager.AddLoginAsync(user, info);
                        if (addLoginResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            // Если привязка логина не удалась после создания пользователя,
                            // это серьезная проблема, возможно, стоит удалить созданного пользователя.
                            // Но для простоты сейчас просто добавим ошибки.
                            foreach (var error in addLoginResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            // Можно добавить логирование здесь
                            ViewData["ReturnUrl"] = returnUrl;
                            ViewData["LoginProvider"] = info.LoginProvider;
                            return View(model);
                        }
                    }
                    else
                    {
                        // Ошибка при создании пользователя
                        foreach (var error in createResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        ViewData["ReturnUrl"] = returnUrl;
                        ViewData["LoginProvider"] = info.LoginProvider;
                        return View(model);
                    }
                }
                else
                {
                    // User found. Now link the external login.
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (addLoginResult.Succeeded)
                    {
                        // Успешно привязали внешний логин к существующему аккаунту.
                        // Так как email подтвержден внешним провайдером, помечаем его как подтвержденный в нашей системе.
                        if (!await _userManager.IsEmailConfirmedAsync(user))
                        {
                            user.EmailConfirmed = true;
                            await _userManager.UpdateAsync(user);
                        }
                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        foreach (var error in addLoginResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                            if (error.Code == "LoginAlreadyAssociated")
                            {
                                ModelState.AddModelError(string.Empty, $"Внешний логин уже привязан к другому аккаунту. Пожалуйста, войдите с привязанным аккаунтом.");
                            }
                        }
                        ViewData["ReturnUrl"] = returnUrl;
                        ViewData["LoginProvider"] = info.LoginProvider;
                        return View(model);
                    }
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;
            return View(model);
        }

        /// <summary>
        /// Handles user logout.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Обрабатывает подтверждение email по ссылке из письма.
        /// </summary>
        /// <param name="userId">ID пользователя.</param>
        /// <param name="code">Токен подтверждения.</param>
        /// <param name="returnUrl">URL для перенаправления после подтверждения.</param>
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code, string returnUrl = null)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Index", "Home"); // Или на страницу ошибки
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Не удалось загрузить пользователя с ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            ViewBag.StatusMessage = result.Succeeded ? "Спасибо за подтверждение вашего email." : "Ошибка подтверждения email.";

            if (result.Succeeded)
            {
                // Если email успешно подтвержден, можно сразу войти пользователя
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }
            else
            {
                // Если подтверждение не удалось, можно показать страницу с ошибкой
                return View("Error"); // Убедитесь, что у вас есть представление Error.cshtml
            }
        }


        /// <summary>
        /// Отображает страницу для повторной отправки подтверждения Email.
        /// </summary>
        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            // Возвращаем пустую модель.
            return View(new ResendEmailConfirmationViewModel());
        }

        /// <summary>
        /// Обрабатывает запрос на повторную отправку подтверждения Email.
        /// </summary>
        /// <param name="model">Модель с Email пользователя.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Для предотвращения перечисления пользователей, всегда возвращаем общее сообщение об успехе.
                // Не разглашаем, существует ли пользователь или подтвержден ли его Email.
                ViewBag.StatusMessage = "Если ваш Email зарегистрирован, вам будет отправлено письмо для подтверждения.";
                ViewBag.StatusMessageType = "success";
                return View(model);
            }

            // Проверяем, подтвержден ли уже email
            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                ViewBag.StatusMessage = "Ваш Email уже подтвержден. Вы можете войти в свой аккаунт.";
                ViewBag.StatusMessageType = "info"; // Или success, в зависимости от желаемого сообщения
                return View(model);
            }

            // Генерируем новый токен подтверждения email
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Создаем URL обратного вызова для подтверждения email
            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                values: new { userId = user.Id, code = code, email = model.Email }, // Передаем email для удобства в RegisterConfirmation
                protocol: Request.Scheme);

            // Отправляем письмо с подтверждением
            await _emailSender.SendEmailAsync(
                model.Email,
                "Повторное подтверждение Email на MajorAuthor",
                $"Пожалуйста, подтвердите ваш аккаунт, перейдя по ссылке: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Подтвердить Email</a>.");

            ViewBag.StatusMessage = "Письмо с подтверждением отправлено. Пожалуйста, проверьте свой Email (включая папку со спамом).";
            ViewBag.StatusMessageType = "success";
            return View(model);
        }

        /// <summary>
        /// Отображает страницу "Забыли пароль".
        /// </summary>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        /// <summary>
        /// Обрабатывает запрос на сброс пароля.
        /// </summary>
        /// <param name="model">Модель с Email для сброса пароля.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Для предотвращения перечисления пользователей, всегда возвращаем общее сообщение об успехе.
                // Не разглашаем, существует ли пользователь или подтвержден ли его Email.
                ViewBag.StatusMessage = "Если ваш Email зарегистрирован и подтвержден, вам будет отправлено письмо для сброса пароля.";
                ViewBag.StatusMessageType = "success";
                return View(model);
            }

            // Генерируем токен сброса пароля
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Создаем URL обратного вызова для сброса пароля
            var callbackUrl = Url.Action(
                "ResetPassword", // Вам нужно будет создать метод ResetPassword и соответствующее представление
                "Account",
                values: new { code = code, email = model.Email },
                protocol: Request.Scheme);

            // Отправляем письмо со ссылкой для сброса пароля
            await _emailSender.SendEmailAsync(
                model.Email,
                "Сброс пароля для MajorAuthor",
                $"Пожалуйста, сбросьте ваш пароль, перейдя по ссылке: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Сбросить пароль</a>.");

            ViewBag.StatusMessage = "Письмо для сброса пароля отправлено. Пожалуйста, проверьте свой Email (включая папку со спамом).";
            ViewBag.StatusMessageType = "success";
            return View(model);
        }

        /// <summary>
        /// Отображает страницу сброса пароля, когда пользователь переходит по ссылке из письма.
        /// </summary>
        /// <param name="code">Токен сброса пароля.</param>
        /// <param name="email">Email пользователя.</param>
        [HttpGet]
        public IActionResult ResetPassword(string code = null, string email = null)
        {
            if (code == null || email == null)
            {
                return RedirectToAction("Index", "Home"); // Или на страницу ошибки
            }
            // Передаем токен и email во ViewModel
            var model = new ResetPasswordViewModel { Code = code, Email = email };
            return View(model);
        }

        /// <summary>
        /// Обрабатывает отправку формы сброса пароля.
        /// </summary>
        /// <param name="model">Модель с новым паролем, подтверждением, кодом и Email.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Для предотвращения перечисления пользователей, всегда возвращаем общее сообщение об успехе.
                ViewBag.StatusMessage = "Пароль успешно сброшен."; // Общее сообщение
                ViewBag.StatusMessageType = "success";
                return View(model);
            }

            // Сброс пароля с использованием токена
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                ViewBag.StatusMessage = "Ваш пароль был успешно сброшен. Теперь вы можете войти.";
                ViewBag.StatusMessageType = "success";
                // Можно перенаправить на страницу входа
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
            return View(); // Requires Views/Account/LoginWith2fa.cshtml
        }

        [HttpGet]
        public IActionResult Lockout()
        {
            return View(); // Requires Views/Account/Lockout.cshtml
        }
    }
}
