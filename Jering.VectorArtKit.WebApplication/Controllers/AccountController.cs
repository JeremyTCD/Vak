using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Jering.VectorArtKit.WebApplication.ViewModels;
using Jering.AccountManagement.DatabaseInterface;
using Microsoft.AspNetCore.Http.Authentication;
using System.Data.SqlClient;
using Jering.AccountManagement.Security;
using Jering.VectorArtKit.WebApplication.BusinessModel;
using Jering.VectorArtKit.WebApplication.Filters;
using Microsoft.Extensions.Options;
using Jering.VectorArtKit.WebApplication.ViewModels.Shared;

namespace Jering.VectorArtKit.WebApplication.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private VakAccountRepository _vakAccountRepository;
        private IAccountSecurityServices<VakAccount> _accountSecurityServices;
        private ViewModelOptions _viewModelOptions;

        public AccountController(
            IAccountRepository<VakAccount> vakAccountRepository,
            IAccountSecurityServices<VakAccount> accountSecurityServices,
            IOptions<ViewModelOptions> viewModelOptionsAccessor
            )
        {
            _vakAccountRepository = (VakAccountRepository)vakAccountRepository;
            _accountSecurityServices = accountSecurityServices;
            _viewModelOptions = viewModelOptionsAccessor?.Value;
        }

        /// <summary>
        /// GET: /Account/Login
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns>
        /// Login view with anti-forgery token and cookie.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        [SetSignedInAccount]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Post: /Account/Login
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns>
        /// Bad request if anti-forgery credentials are invalid.
        /// Login view with error message if model state or login credentials are invalid. 
        /// Home index view or return Url view with an application cookie if login is successful.
        /// Redirects to /Account/VerifyCode with a two factor cookie if two factor is required. 
        /// Redirects to /Account/EmailConfirmation with an email confirmation cookie if email confirmation is required.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                PasswordSignInResult passwordSignInResult = await _accountSecurityServices.PasswordSignInAsync(model.Email,
                    model.Password,
                    new AuthenticationProperties() { IsPersistent = model.RememberMe });

                if (passwordSignInResult == PasswordSignInResult.TwoFactorRequired)
                {
                    return RedirectToAction(nameof(VerifyCode), new { IsPersistent = model.RememberMe, ReturnUrl = returnUrl});
                }
                else if (passwordSignInResult == PasswordSignInResult.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
                else if(passwordSignInResult == PasswordSignInResult.EmailConfirmationRequired)
                {
                    return RedirectToAction(nameof(EmailConfirmation));
                }
            }

            ModelState.AddModelError(string.Empty, _viewModelOptions.Login_Failed);
            return View(model);
        }

        /// <summary>
        /// GET: /Account/Register
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns>
        /// Register view with anti-forgery token and cookie.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// POST: /Account/Register
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns>
        /// Bad request if anti-forgery credentials are invalid.
        /// Register view with error messages if model state is invalid. 
        /// Register view with error message if create account fails. 
        /// Redirects to /Account/EmailConfirmation with an email confirmation cookie if registration succeeds.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                CreateAccountResult createAccountResult = await _accountSecurityServices.CreateAccountAsync(model.Email, model.Password);
             
                if(createAccountResult == CreateAccountResult.Succeeded)
                {
                    return RedirectToAction(nameof(EmailConfirmation));
                }

                ModelState.AddModelError(string.Empty, _viewModelOptions.Register_AccountWithEmailExists);
            }

            return View(model);
        }

        /// <summary>
        /// POST: /Account/LogOff
        /// </summary>
        /// <returns>
        /// Redirects to /Home/Index with set-cookie headers to remove all cookies.
        /// Redirects to /Account/Login if authentication fails.
        /// Bad request if anti-forgery credentials are invalid.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _accountSecurityServices.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // GET: /Account/ConfirmEmailRequired
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> EmailConfirmation()
        {
            VakAccount vakAccount = await _accountSecurityServices.GetEmailConfirmationAccountAsync();
            if (vakAccount == null)
            {
                return View("Error");
            }

            // TODO: must provide means to change email and resend token link
            return View(new EmailConfirmationViewModel { Email = vakAccount.Email });
        }

        // TODO: resends confirmation email
        // POST: /Account/ConfirmEmailRequired
        //[HttpPost]
        //[AllowAnonymous]
        //public IActionResult EmailConfirmation(int accountId)
        //{
        //    return View("ConfirmEmail");
        //}

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            ConfirmEmailResult emailConfirmationResult = await _accountSecurityServices.ConfirmEmailAsync(token);

            if (emailConfirmationResult == ConfirmEmailResult.Succeeded)
            {
                return View();
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

        //
        // GET: /Account/VerifyCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(bool rememberMe, string returnUrl = null)
        {
            VakAccount vakAccount = await _accountSecurityServices.GetTwoFactorAccountAsync();
            if (vakAccount == null)
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel {IsPersistent = rememberMe, ReturnUrl = returnUrl});
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TwoFactorSignInResult twoFactorSignInResult = await _accountSecurityServices.TwoFactorSignInAsync(model.Token, model.IsPersistent);

            if (twoFactorSignInResult == TwoFactorSignInResult.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid code.");
            return View(model);
        }

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
            // This prevents open redirection attacks - http://www.asp.net/mvc/overview/security/preventing-open-redirection-attacks
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
