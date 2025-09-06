// Project: MajorAuthor.Web
// File: Services/AccountService.cs
using MajorAuthor.Data;
using MajorAuthor.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using System.Security.Claims;
using System.Linq;

namespace MajorAuthor.Services
{
    /// <summary>
    /// Service for handling account-related business logic.
    /// This service is now completely independent of the MVC context.
    /// </summary>
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

        public async Task<SignInResult> LoginWithPasswordAsync(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded && !await _userManager.IsEmailConfirmedAsync(user))
                {
                    await _signInManager.SignOutAsync();
                    return SignInResult.Failed;
                }
                return result;
            }
            return SignInResult.Failed;
        }

        public async Task<ExternalLoginResult> HandleExternalLoginCallbackAsync(ExternalLoginInfo info, string returnUrl)
        {
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                return ExternalLoginResult.Succeeded;
            }
            if (result.IsLockedOut)
            {
                return ExternalLoginResult.Lockout;
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                var logins = await _userManager.GetLoginsAsync(existingUser);
                if (logins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey))
                {
                    return ExternalLoginResult.Succeeded;
                }
                var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                if (addLoginResult.Succeeded)
                {
                    await _signInManager.SignInAsync(existingUser, isPersistent: false, info.LoginProvider);
                    return ExternalLoginResult.Succeeded;
                }
                return ExternalLoginResult.LoginAlreadyAssociated;
            }

            return ExternalLoginResult.RequiresConfirmation;
        }

        public async Task<IdentityResult> ConfirmExternalLoginAsync(ExternalLoginInfo? info, ExternalLoginConfirmationViewModel model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                }
            }
            return result;
        }

        public async Task<(IdentityResult, ApplicationUser)> RegisterAndSendConfirmationAsync(RegisterViewModel model, string callbackUrl)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                callbackUrl = callbackUrl.Replace("REPLACE_USER_ID", user.Id).Replace("REPLACE_CODE", code);
                var emailSubject = "Подтверждение Email";
                var emailMessage = $"Пожалуйста, подтвердите ваш Email, перейдя по <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>этой ссылке</a>.";
                await _emailSender.SendEmailAsync(model.Email, emailSubject, emailMessage);
            }

            return (result, user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                return IdentityResult.Failed(new IdentityError { Description = "User ID and code are required." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            return result;
        }

        public async Task<bool> ResendEmailConfirmationAsync(ResendEmailConfirmationViewModel model, string callbackUrl)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || await _userManager.IsEmailConfirmedAsync(user))
            {
                return true;
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            callbackUrl = callbackUrl.Replace("REPLACE_USER_ID", user.Id).Replace("REPLACE_CODE", code);
            var emailSubject = "Повторное письмо для подтверждения Email";
            var emailMessage = $"Пожалуйста, подтвердите ваш Email, перейдя по <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>этой ссылке</a>.";
            await _emailSender.SendEmailAsync(model.Email, emailSubject, emailMessage);
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordViewModel model, string callbackUrl)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return true;
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            callbackUrl = callbackUrl.Replace("REPLACE_CODE", code).Replace("REPLACE_EMAIL", user.Email);
            var emailSubject = "Сброс пароля";
            var emailMessage = $"Пожалуйста, сбросьте ваш пароль, нажав на <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>эту ссылку</a>.";
            await _emailSender.SendEmailAsync(model.Email, emailSubject, emailMessage);
            return true;
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            return result;
        }
    }
}
