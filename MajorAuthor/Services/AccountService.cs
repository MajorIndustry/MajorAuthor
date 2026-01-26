// Project: MajorAuthor.Web
// File: Services/AccountService.cs

using MajorAuthor.Data;
using MajorAuthor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MajorAuthor.Services
{
    public class AccountService : IAccountService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AccountService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // --- Вспомогательные методы (Helper Methods) ---

        private async Task<(ApplicationUser? user, bool multiple)> SafeFindByEmailAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return (null, false);
            var list = await _userManager.Users.Where(u => u.Email == email).ToListAsync();
            if (list.Count == 0) return (null, false);
            if (list.Count == 1) return (list[0], false);
            return (list[0], true);
        }

        private async Task SendEmailConfirmationAsync(ApplicationUser user, string callbackUrl)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            callbackUrl = QueryHelpers.AddQueryString(callbackUrl, new Dictionary<string, string?>
            {
                { "userId", user.Id },
                { "code", code }
            });

            var emailSubject = "Подтверждение Email";
            var emailMessage = $@"
                <h3>Добро пожаловать в MajorAuthor!</h3>
                <p>Пожалуйста, подтвердите ваш email, перейдя по ссылке ниже:</p>
                <p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Подтвердить email</a></p>
                <p>Если вы не регистрировались в нашем сервисе, проигнорируйте это письмо.</p>
                <br>
                <p>С уважением,<br>Команда MajorAuthor</p>";

            await _emailSender.SendEmailAsync(user.Email, emailSubject, emailMessage);
        }

        private (string? firstName, string? lastName) ExtractNameFromExternalInfo(ExternalLoginInfo info)
        {
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

            if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                return (firstName, lastName);
            }

            var fullName = info.Principal.FindFirstValue(ClaimTypes.Name)
                           ?? info.Principal.FindFirstValue("name");

            if (!string.IsNullOrEmpty(fullName))
            {
                var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2)
                {
                    return (parts[0], parts[1]);
                }
                else if (parts.Length > 2)
                {
                    return (parts[0], string.Join(" ", parts.Skip(1)));
                }
                else if (parts.Length == 1)
                {
                    return (parts[0], null);
                }
            }

            return (null, null);
        }

        private (string? firstName, string? lastName) ExtractNameFromExternalTempData(ExternalLoginTempData tempData)
        {
            return (tempData.FirstName, tempData.LastName);
        }

        private async Task<string> EnsureUniqueUserName(string baseUserName)
        {
            var userName = baseUserName;
            var counter = 1;
            while (await _userManager.FindByNameAsync(userName) != null)
            {
                userName = $"{baseUserName}{counter}";
                counter++;
                if (counter > 100)
                {
                    userName = $"{baseUserName}_{Guid.NewGuid().ToString().Substring(0, 4)}";
                    break;
                }
            }
            return userName;
        }

        private async Task<string> GenerateUserNameFromEmail(string email)
        {
            var baseUserName = email.Split('@')[0];
            baseUserName = new string(baseUserName.Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_').ToArray());

            if (string.IsNullOrEmpty(baseUserName))
                baseUserName = "user";

            return await EnsureUniqueUserName(baseUserName.ToLowerInvariant());
        }

        private string ExtractProfilePictureUrl(ExternalLoginInfo info)
        {
            if (info.LoginProvider == "Google") return info.Principal.FindFirstValue("picture");
            if (info.LoginProvider == "Yandex")
            {
                var avatarId = info.Principal.FindFirstValue("default_avatar_id");
                if (!string.IsNullOrEmpty(avatarId)) return $"https://avatars.yandex.net/get-yapic/{avatarId}/islands-200";
                return info.Principal.FindFirstValue("picture");
            }
            return info.Principal.FindFirstValue("picture");
        }

        private string ExtractProfilePictureUrlFromTempData(ExternalLoginTempData tempData)
        {
            return tempData.ProfilePictureUrl;
        }

        private async Task UpdateUserFromExternalInfo(ExternalLoginInfo info)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return;

            var hasChanges = false;
            var newProfilePictureUrl = ExtractProfilePictureUrl(info);
            if (!string.IsNullOrEmpty(newProfilePictureUrl) && user.ProfilePictureUrl != newProfilePictureUrl)
            {
                user.ProfilePictureUrl = newProfilePictureUrl;
                hasChanges = true;
            }

            var (newFirstName, newLastName) = ExtractNameFromExternalInfo(info);
            if (!string.IsNullOrEmpty(newFirstName) && user.FirstName != newFirstName)
            {
                user.FirstName = newFirstName;
                hasChanges = true;
            }
            if (!string.IsNullOrEmpty(newLastName) && user.LastName != newLastName)
            {
                user.LastName = newLastName;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _userManager.UpdateAsync(user);
            }
        }

        // --- Основные публичные методы ---

        public async Task<ExternalLoginCallbackResult> HandleExternalLoginCallbackAsync(ExternalLoginInfo info, string returnUrl)
        {
            try
            {
                // 1. Пытаемся войти с помощью внешнего логина
                var signInResult = await _signInManager.ExternalLoginSignInAsync(
                    info.LoginProvider,
                    info.ProviderKey,
                    isPersistent: false,
                    bypassTwoFactor: true);

                if (signInResult.Succeeded)
                {
                    await UpdateUserFromExternalInfo(info);
                    return ExternalLoginCallbackResult.Succeeded(returnUrl);
                }

                if (signInResult.IsLockedOut)
                {
                    return ExternalLoginCallbackResult.Lockout();
                }

                // 2. Если вход не удался, ищем пользователя по email
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                if (!string.IsNullOrEmpty(email))
                {
                    // Ищем существующего пользователя по email
                    var existingUser = await _userManager.FindByEmailAsync(email);
                    if (existingUser != null)
                    {
                        // Найден существующий пользователь - привязываем внешний логин и входим
                        var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                        if (addLoginResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(existingUser, isPersistent: false);
                            await UpdateUserFromExternalInfo(info);
                            return ExternalLoginCallbackResult.Succeeded(returnUrl);
                        }
                        return ExternalLoginCallbackResult.Error("Ошибка при привязке внешнего аккаунта.");
                    }
                    else
                    {
                        // Пользователь не найден - создаем нового
                        var result = await AutoCreateExternalUserAsync(info);
                        if (result.Succeeded)
                        {
                            return ExternalLoginCallbackResult.Succeeded(returnUrl);
                        }
                        return ExternalLoginCallbackResult.Error(result.Errors.FirstOrDefault()?.Description ?? "Ошибка при создании пользователя.");
                    }
                }

                // 3. Email отсутствует - возвращаем данные для запроса email
                var tempData = new ExternalLoginTempData
                {
                    LoginProvider = info.LoginProvider,
                    ProviderKey = info.ProviderKey,
                    FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                    LastName = info.Principal.FindFirstValue(ClaimTypes.Surname),
                    ProfilePictureUrl = ExtractProfilePictureUrl(info),
                    ReturnUrl = returnUrl
                };

                return ExternalLoginCallbackResult.EmailRequired(tempData);
            }
            catch (Exception ex)
            {
                return ExternalLoginCallbackResult.Error($"Произошла непредвиденная ошибка: {ex.Message}");
            }
        }

        public async Task<IdentityResult> AutoCreateExternalUserAsync(ExternalLoginInfo info)
        {
            try
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Email не предоставлен внешним провайдером." });
                }

                // Дополнительная проверка: убеждаемся, что пользователь действительно не существует
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    // Если пользователь найден, привязываем внешний логин и входим
                    var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                    if (addLoginResult.Succeeded)
                    {
                        // Если email не подтвержден, оставляем его неподтвержденным
                        // (пользователь должен подтвердить email через стандартный процесс)
                        await UpdateUserFromExternalInfo(info);
                        await _signInManager.SignInAsync(existingUser, isPersistent: false, info.LoginProvider);
                        return IdentityResult.Success;
                    }
                    return addLoginResult;
                }

                // Создаем нового пользователя с подтвержденным email
                // (так как провайдер уже подтвердил email)
                var (firstName, lastName) = ExtractNameFromExternalInfo(info);
                var userName = await GenerateUserNameFromEmail(email);
                var profilePictureUrl = ExtractProfilePictureUrl(info);

                var user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true, // Email подтвержден провайдером
                    FirstName = firstName,
                    LastName = lastName,
                    ProfilePictureUrl = profilePictureUrl,
                    RegistrationDate = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return IdentityResult.Success;
                    }
                    else
                    {
                        // Если не удалось привязать логин, удаляем пользователя
                        await _userManager.DeleteAsync(user);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Критическая ошибка при создании пользователя: {ex.Message}" });
            }
        }

        public async Task<ExternalLoginJsonResult> CompleteExternalRegistrationAsync(string email, ExternalLoginTempData tempData, string callbackUrl)
        {
            try
            {
                // Проверяем, существует ли пользователь с таким Email
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    // Пользователь существует - привязываем внешний логин
                    var loginInfo = new UserLoginInfo(tempData.LoginProvider, tempData.ProviderKey, tempData.LoginProvider);
                    var addLoginResult = await _userManager.AddLoginAsync(existingUser, loginInfo);

                    if (addLoginResult.Succeeded)
                    {
                        // Если email не подтвержден, отправляем письмо для подтверждения
                        if (!existingUser.EmailConfirmed)
                        {
                            await SendEmailConfirmationAsync(existingUser, callbackUrl);

                            // НЕ входим в аккаунт, пока email не подтвержден
                            return new ExternalLoginJsonResult
                            {
                                Status = "email_confirmation_required",
                                Message = "Внешний аккаунт успешно привязан. Для завершения регистрации подтвердите ваш email. Письмо с инструкциями отправлено.",
                                RedirectUrl = null // Контроллер обработает этот случай
                            };
                        }
                        else
                        {
                            // Email уже подтвержден - обычный вход
                            await _signInManager.SignInAsync(existingUser, isPersistent: false);
                            return new ExternalLoginJsonResult
                            {
                                Status = "success",
                                Message = "Вход выполнен успешно!",
                                RedirectUrl = tempData.ReturnUrl
                            };
                        }
                    }
                    else
                    {
                        return new ExternalLoginJsonResult
                        {
                            Status = "error",
                            Message = "Ошибка при привязке внешнего аккаунта.",
                            Errors = addLoginResult.Errors.Select(e => e.Description)
                        };
                    }
                }

                // Создаем нового пользователя
                var (firstName, lastName) = ExtractNameFromExternalTempData(tempData);
                var userName = await GenerateUserNameFromEmail(email);

                var user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = false, // Email не подтвержден - нужно отправить письмо
                    FirstName = firstName,
                    LastName = lastName,
                    ProfilePictureUrl = tempData.ProfilePictureUrl,
                    RegistrationDate = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    var loginInfo = new UserLoginInfo(tempData.LoginProvider, tempData.ProviderKey, tempData.LoginProvider);
                    var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);

                    if (addLoginResult.Succeeded)
                    {
                        // Отправляем письмо для подтверждения email
                        await SendEmailConfirmationAsync(user, callbackUrl);

                        // НЕ входим в аккаунт, пока email не подтвержден
                        return new ExternalLoginJsonResult
                        {
                            Status = "email_confirmation_required",
                            Message = "Регистрация выполнена успешно! Для завершения регистрации подтвердите ваш email. Письмо с инструкциями отправлено.",
                            RedirectUrl = null // Контроллер обработает этот случай
                        };
                    }
                    else
                    {
                        await _userManager.DeleteAsync(user);
                        return new ExternalLoginJsonResult
                        {
                            Status = "error",
                            Message = "Ошибка при привязке внешнего аккаунта.",
                            Errors = addLoginResult.Errors.Select(e => e.Description)
                        };
                    }
                }

                return new ExternalLoginJsonResult
                {
                    Status = "error",
                    Message = "Ошибка при создании пользователя.",
                    Errors = createResult.Errors.Select(e => e.Description)
                };
            }
            catch (Exception ex)
            {
                return new ExternalLoginJsonResult
                {
                    Status = "error",
                    Message = $"Произошла непредвиденная ошибка: {ex.Message}"
                };
            }
        }

        // --- Остальные методы ---

        public async Task<SignInResult> LoginWithPasswordAsync(LoginViewModel model)
        {
            var (user, multiple) = await SafeFindByEmailAsync(model.Email);
            if (multiple || user == null) return SignInResult.Failed;

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded && !await _userManager.IsEmailConfirmedAsync(user))
            {
                await _signInManager.SignOutAsync();
                return SignInResult.Failed;
            }
            return result;
        }

        public async Task<(IdentityResult, ApplicationUser)> RegisterAndSendConfirmationAsync(RegisterViewModel model, string callbackUrl)
        {
            var userName = await GenerateUserNameFromEmail(model.Email);

            var (existingByEmail, multipleByEmail) = await SafeFindByEmailAsync(model.Email);
            var errors = new List<IdentityError>();

            if (multipleByEmail) errors.Add(new IdentityError { Code = "MultipleUsersWithSameEmail", Description = "В базе найдено несколько пользователей с таким Email. Обратитесь к администратору." });
            else if (existingByEmail != null) errors.Add(new IdentityError { Code = "DuplicateEmail", Description = "Пользователь с таким Email уже существует." });

            if (errors.Any()) return (IdentityResult.Failed(errors.ToArray()), new ApplicationUser());

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                RegistrationDate = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded) await SendEmailConfirmationAsync(user, callbackUrl);

            return (result, user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
                return IdentityResult.Failed(new IdentityError { Description = "User ID and code are required." });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            return await _userManager.ConfirmEmailAsync(user, code);
        }

        public async Task<bool> ResendEmailConfirmationAsync(ResendEmailConfirmationViewModel model, string callbackUrl)
        {
            var (user, multiple) = await SafeFindByEmailAsync(model.Email);
            if (multiple || user == null || await _userManager.IsEmailConfirmedAsync(user)) return true;

            await SendEmailConfirmationAsync(user, callbackUrl);
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordViewModel model, string callbackUrl)
        {
            var (user, multiple) = await SafeFindByEmailAsync(model.Email);
            if (multiple || user == null || !(await _userManager.IsEmailConfirmedAsync(user))) return true;

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            callbackUrl = QueryHelpers.AddQueryString(callbackUrl.Split('?')[0], new Dictionary<string, string?>
            {
                { "email", user.Email },
                { "code", code }
            });

            var emailSubject = "Сброс пароля";
            var emailMessage = $"Пожалуйста, сбросьте ваш пароль, нажав на <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>эту ссылку</a>.";
            await _emailSender.SendEmailAsync(model.Email, emailSubject, emailMessage);
            return true;
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            return await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
        }
    }

    public class ExternalLoginCallbackResult
    {
        public bool Success { get; set; }
        public string RedirectUrl { get; set; }
        public bool _Lockout { get; set; }
        public bool _EmailRequired { get; set; }
        public ExternalLoginTempData TempData { get; set; }
        public string _Error { get; set; }

        public static ExternalLoginCallbackResult Succeeded(string redirectUrl)
        {
            return new ExternalLoginCallbackResult { Success = true, RedirectUrl = redirectUrl };
        }

        public static ExternalLoginCallbackResult Lockout()
        {
            return new ExternalLoginCallbackResult { _Lockout = true };
        }

        public static ExternalLoginCallbackResult EmailRequired(ExternalLoginTempData tempData)
        {
            return new ExternalLoginCallbackResult { _EmailRequired = true, TempData = tempData };
        }

        public static ExternalLoginCallbackResult Error(string error)
        {
            return new ExternalLoginCallbackResult { Success = false, _Error = error };
        }
    }
}