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

namespace Jering.VectorArtKit.WebApplication.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private VakAccountRepository _vakAccountRepository;
        private IAccountSecurityServices<VakAccount> _accountSecurityServices;
        //private readonly IEmailSender _emailSender;

        public AccountController(
            IAccountRepository<VakAccount> vakAccountRepository,
            IAccountSecurityServices<VakAccount> accountSecurityServices
            )
        {
            if (vakAccountRepository == null)
            {
                throw new ArgumentNullException(nameof(vakAccountRepository));
            }

            if (accountSecurityServices == null)
            {
                throw new ArgumentNullException(nameof(accountSecurityServices));
            }

            _vakAccountRepository = (VakAccountRepository)vakAccountRepository;
            _accountSecurityServices = accountSecurityServices;
            //_emailSender = emailSender;
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
        /// Login view if model state is invalid. 
        /// Login view with error message if login credentials are invalid. 
        /// Home index view or return Url view with an application cookie if login is successful.
        /// Redirects to /Account/VerifyCode with a two factor cookie if two factor is required. 
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                ApplicationSignInResult applicationSignInResult = await _accountSecurityServices.ApplicationPasswordSignInAsync(model.Email,
                    model.Password,
                    new AuthenticationProperties() { IsPersistent = model.RememberMe });

                if (applicationSignInResult == ApplicationSignInResult.TwoFactorRequired)
                {
                    return RedirectToAction(nameof(VerifyCode), new { IsPersistent = model.RememberMe, ReturnUrl = returnUrl});
                }
                else if (applicationSignInResult == ApplicationSignInResult.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
                else if(applicationSignInResult == ApplicationSignInResult.EmailConfirmationRequired)
                {
                    // TODO: handle email confirmation required. Ideally return an emailconfirmation cookie and redirect to an email confirmation view.
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            // Need to think about disposing of accountSecurityServices 
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
                // TODO: don't immediately sign user in, redirect to email confirmation page
                await _accountSecurityServices.ApplicationSignInAsync(account, new AuthenticationProperties { IsPersistent = true});
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
