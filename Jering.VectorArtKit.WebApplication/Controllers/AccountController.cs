using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Jering.VectorArtKit.WebApplication.Models;
using Jering.AccountManagement.DatabaseInterface;
using Microsoft.AspNetCore.Http.Authentication;
using System.Data.SqlClient;
using Jering.AccountManagement.Security;
using Jering.VectorArtKit.WebApplication.BusinessModel;

namespace Jering.VectorArtKit.WebApplication.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private VakAccountRepository _vakAccountRepository;
        private AccountSecurityServices<VakAccount> _accountSecurityServices;
        //private readonly IEmailSender _emailSender;

        public AccountController(
            IAccountRepository<VakAccount> vakAccountRepository,
            AccountSecurityServices<VakAccount> accountSecurityServices
            //IEmailSender emailSender
            )
        {
            if(vakAccountRepository == null)
            {
                throw new ArgumentNullException(nameof(vakAccountRepository));
            }

            if (accountSecurityServices == null)
            {
                throw new ArgumentNullException(nameof(accountSecurityServices));
            }

            _vakAccountRepository = (VakAccountRepository) vakAccountRepository;
            _accountSecurityServices = accountSecurityServices;
            //_emailSender = emailSender;
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                if (await _accountSecurityServices.PasswordSignInAsync(model.Email, model.Password, new AuthenticationProperties() { IsPersistent = model.RememberMe }))
                {
                    return RedirectToLocal(returnUrl);
                }
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                //}

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                VakAccount account;

                try
                {
                    account = await _vakAccountRepository.CreateAccountAsync(model.Email, model.Password);
                }
                catch (SqlException sqlException)
                {
                    // TODO: handle exceptions
                    if (sqlException.Class == 0)
                    {
                        //AddErrors(result);
                    }
                    return View(model);
                }

                await _accountSecurityServices.SendConfirmationEmailAsync(account);
                // TODO IsPersistent should be set to remember me
                await _accountSecurityServices.SignInAsync(account, new AuthenticationProperties { IsPersistent = true, });
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // Model state not valid, return form
                return View(model);
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _accountSecurityServices.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(int accountId, string token)
        {
            // TODO: Instead of grabbing a bool here, we should get a result object so that information is available if things go wrong
            // e.g token expired.
            if (await _accountSecurityServices.ConfirmEmailAsync(accountId, token))
            {
                return View("ConfirmEmail");
            }

            return View("Error");
        }

        //
        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _vakAccountRepository.GetAccountByEmailAsync(model.Email);
                if (account == null || !account.EmailConfirmed)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                await _accountSecurityServices.SendConfirmationEmailAsync(account);
                return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            VakAccount account = await _vakAccountRepository.GetAccountByEmailAsync(model.Email);
            if (account == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            // accountManager should take care of this, code needs to be checked first
            if (await _accountSecurityServices.UpdatePasswordAsync(account, model.Password, model.Code))
            {
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }
            //AddErrors(result);
            return View();
        }

        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // TODO 2 Factor
        // GET: /Account/SendCode
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<ActionResult> SendCode(string returnUrl = null, bool reaccountMe = false)
        //{
        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //    if (user == null)
        //    {
        //        return View("Error");
        //    }
        //    var userFactors = await _accountRepository.GetValidTwoFactorProvidersAsync(user);
        //    var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
        //    return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, ReaccountMe = reaccountMe });
        //}

        // TODO 2 Factor
        // POST: /Account/SendCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SendCode(SendCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View();
        //    }

        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //    if (user == null)
        //    {
        //        return View("Error");
        //    }

        //    // Generate the token and send it
        //    var code = await _accountRepository.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
        //    if (string.IsNullOrWhiteSpace(code))
        //    {
        //        return View("Error");
        //    }

        //    var message = "Your security code is: " + code;
        //    if (model.SelectedProvider == "Email")
        //    {
        //        await _emailSender.SendEmailAsync(await _accountRepository.GetEmailAsync(user), "Security Code", message);
        //    }
        //    else if (model.SelectedProvider == "Phone")
        //    {
        //        await _smsSender.SendSmsAsync(await _accountRepository.GetPhoneNumberAsync(user), message);
        //    }

        //    return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, ReaccountMe = model.ReaccountMe });
        //}

        // TODO 2 Factor
        // GET: /Account/VerifyCode
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> VerifyCode(string provider, bool reaccountMe, string returnUrl = null)
        //{
        //    // Require that the user has already logged in via username/password or external login
        //    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        //    if (user == null)
        //    {
        //        return View("Error");
        //    }
        //    return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, ReaccountMe = reaccountMe });
        //}

        // TODO 2 Factor
        //// POST: /Account/VerifyCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // The following code protects for brute force attacks against the two factor codes.
        //    // If a user enters incorrect codes for a specified amount of time then the user account
        //    // will be locked out for a specified amount of time.
        //    var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.ReaccountMe, model.ReaccountBrowser);
        //    if (result.Succeeded)
        //    {
        //        return RedirectToLocal(model.ReturnUrl);
        //    }
        //    if (result.IsLockedOut)
        //    {
        //        _logger.LogWarning(7, "User account locked out.");
        //        return View("Lockout");
        //    }
        //    else
        //    {
        //        ModelState.AddModelError(string.Empty, "Invalid code.");
        //        return View(model);
        //    }
        //}

        #region Helpers

        //private void AddErrors(IdentityResult result)
        //{
        //    foreach (var error in result.Errors)
        //    {
        //        ModelState.AddModelError(string.Empty, error.Description);
        //    }
        //}

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}
