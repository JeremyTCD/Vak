using Jering.AccountManagement.DatabaseInterface;
using Jering.AccountManagement.Security;
using Jering.Mail;
using Jering.VectorArtKit.WebApplication.BusinessModel;
using Jering.VectorArtKit.WebApplication.Filters;
using Jering.VectorArtKit.WebApplication.Resources;
using Jering.VectorArtKit.WebApplication.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private IAccountSecurityServices<VakAccount> _accountSecurityServices;
        private IAccountRepository<VakAccount> _vakAccountRepository;
        private IEmailServices _emailServices;
        private StringOptions _stringOptions;
        private string _confirmEmailPurpose = "ConfirmEmail";
        private string _resetPasswordPurpose = "ResetPassword";
        private string _twoFactorPurpose = "TwoFactor";

        public AccountController(
            IAccountRepository<VakAccount> vakAccountRepository,
            IAccountSecurityServices<VakAccount> accountSecurityServices,
            IEmailServices emailServices,
            IOptions<StringOptions> stringOptionsAccessor
            )
        {
            _vakAccountRepository = vakAccountRepository;
            _accountSecurityServices = accountSecurityServices;
            _emailServices = emailServices;
            _stringOptions = stringOptionsAccessor?.Value;
        }

        /// <summary>
        /// GET: /Account/SignUp
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns>
        /// SignUp view with anti-forgery token and cookie.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult SignUp(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// POST: /Account/SignUp
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns>
        /// Bad request if anti-forgery credentials are invalid.
        /// SignUp view with error messages if model state is invalid. 
        /// SignUp view with error message if create account fails. 
        /// Redirects to /Home/Index with an application cookie and sends confirm email email if account is created successfully.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                CreateAccountResult<VakAccount> createAccountResult = await _accountSecurityServices.CreateAccountAsync(model.Email, model.Password);

                if (createAccountResult.Succeeded)
                {
                    string token = await _accountSecurityServices.GetTokenAsync(TokenServiceOptions.DataProtectionTokenService, _confirmEmailPurpose, createAccountResult.Account);
                    string callbackUrl = Url.Action(
                        nameof(AccountController.EmailVerificationConfirmation),
                        nameof(AccountController).Replace("Controller", ""),
                        new { Token = token, AccountId = createAccountResult.Account.AccountId },
                        protocol: HttpContext.Request.Scheme);

                    MimeMessage mimeMessage = _emailServices.CreateMimeMessage(model.Email, _stringOptions.ConfirmEmail_Subject, string.Format(_stringOptions.ConfirmEmail_Message, callbackUrl));
                    await _emailServices.SendEmailAsync(mimeMessage);

                    // TODO: this should eventually redirect to account vakkits page
                    return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", ""));
                }

                ModelState.AddModelError(nameof(SignUpViewModel.Email), _stringOptions.SignUp_AccountWithEmailExists);
            }
            else if (ModelState.ContainsKey(nameof(SignUpViewModel.Password)))
            {
                ModelState.Remove(nameof(SignUpViewModel.Password));
                ModelState.AddModelError(nameof(SignUpViewModel.Password), _stringOptions.Password_Invalid);
            }

            return View(model);
        }

        /// <summary>
        /// GET: /Account/LogIn
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns>
        /// Login view with anti-forgery token and cookie.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        [SetSignedInAccount]
        public IActionResult LogIn(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Post: /Account/LogIn
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns>
        /// Bad request if anti-forgery credentials are invalid.
        /// Login view with error message if model state or login credentials are invalid. 
        /// Home index view or return Url view with an application cookie if login is successful.
        /// Redirects to /Account/VerifyCode with a two factor cookie and sends two factor email if two factor is required. 
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                PasswordSignInResult<VakAccount> passwordSignInResult = await _accountSecurityServices.PasswordSignInAsync(model.Email,
                    model.Password,
                    new AuthenticationProperties() { IsPersistent = model.RememberMe });

                if (passwordSignInResult.TwoFactorRequired)
                {
                    await _accountSecurityServices.CreateTwoFactorCookieAsync(passwordSignInResult.Account);
                    string token = await _accountSecurityServices.GetTokenAsync(TokenServiceOptions.TotpTokenService, _twoFactorPurpose, passwordSignInResult.Account);
                    MimeMessage mimeMessage = _emailServices.CreateMimeMessage(passwordSignInResult.Account.Email, _stringOptions.TwoFactorEmail_Subject, string.Format(_stringOptions.TwoFactorEmail_Message, token));
                    await _emailServices.SendEmailAsync(mimeMessage);

                    return RedirectToAction(nameof(VerifyTwoFactorCode), new { IsPersistent = model.RememberMe, ReturnUrl = returnUrl });
                }
                if (passwordSignInResult.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
            }

            ModelState.AddModelError(string.Empty, _stringOptions.LogIn_Failed);
            return View(model);
        }

        /// <summary>
        /// GET: /Account/VerifyTwoFactorCode
        /// </summary>
        /// <param name="isPersistent"></param>
        /// <param name="returnUrl"></param>
        /// <returns>
        /// Error view if two factor cookie does not exist or is invalid.
        /// VerifyTwoFactorCode view with anti-forgery token and cookie if two factor cookie is valid.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyTwoFactorCode(bool isPersistent, string returnUrl = null)
        {
            VakAccount vakAccount = await _accountSecurityServices.GetTwoFactorAccountAsync();
            if (vakAccount == null)
            {
                return View("Error");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        /// <summary>
        /// POST: /Account/VerifyTwoFactorCode
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns>
        /// VerifyTwoFactorCode view with error message if model state or code is invalid. 
        /// Home index view or return Url view with an application cookie if code is valid.
        /// BadRequest if anti-forgery credentials are invalid.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [SetSignedInAccount]
        public async Task<IActionResult> VerifyTwoFactorCode(VerifyTwoFactorCodeViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                TwoFactorSignInResult twoFactorSignInResult = await _accountSecurityServices.TwoFactorSignInAsync(model.Code, model.IsPersistent);

                if (twoFactorSignInResult.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError(nameof(VerifyTwoFactorCodeViewModel.Code), _stringOptions.VerifyTwoFactorCode_InvalidCode);
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

        /// <summary>
        /// GET: /Account/ForgotPassword
        /// </summary>
        /// <returns>
        /// ForgotPassword view with anti-forgery token and cookie.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// POST: /Account/ForgotPassword
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// ForgotPassword view with error message if model state is invalid.
        /// Redirects to ForgotPasswordConfirmation view and sends reset password email if email is valid.
        /// Redirects to ForgotPasswordConfirmation view but does not send reset password email if email is invalid.
        /// BadRequest if anti-forgery credentials are invalid.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _vakAccountRepository.GetAccountByEmailAsync(model.Email);
                if (account != null)
                {
                    string token = await _accountSecurityServices.GetTokenAsync(TokenServiceOptions.DataProtectionTokenService, _resetPasswordPurpose, account);
                    string callbackUrl = Url.Action(
                        nameof(AccountController.ResetPassword),
                        nameof(AccountController).Replace("Controller", ""),
                        new { Token = token, Email = account.Email },
                        protocol: HttpContext.Request.Scheme);

                    MimeMessage mimeMessage = _emailServices.CreateMimeMessage(account.Email, _stringOptions.ResetPasswordEmail_Subject, string.Format(_stringOptions.ResetPasswordEmail_Message, callbackUrl));
                    await _emailServices.SendEmailAsync(mimeMessage);
                }

                return RedirectToAction(nameof(ForgotPasswordConfirmation), new { Email = model.Email });
            }

            return View(model);
        }

        /// <summary>
        /// GET: /Account/ForgotPasswordConfirmation
        /// </summary>
        /// <returns>
        /// ForgotPasswordConfirmation view.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation(string email)
        {
            return View(model: email);
        }

        /// <summary>
        /// GET: /Account/ResetPassword
        /// </summary>
        /// <returns>
        /// ResetPassword view with anti-forgery token and cookie.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                return View("Error");
            }

            return View(new ResetPasswordViewModel { Email = email });
        }

        /// <summary>
        /// POST: /Account/ResetPassword
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// ResetPassword view with error message if model state is invalid.
        /// Redirects to ResetPasswordConfirmation view and updates password if email, token and model state are valid.
        /// Error view if token or email is invalid.
        /// BadRequest if anti-forgery credentials are invalid.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = model.Email == null ? null : await _vakAccountRepository.GetAccountByEmailAsync(model.Email);
                if (account == null || model.Token == null ||
                    !await _accountSecurityServices.ValidateTokenAsync(TokenServiceOptions.DataProtectionTokenService, _resetPasswordPurpose, account, model.Token) ||
                    !await _vakAccountRepository.UpdateAccountPasswordHashAsync(account.AccountId, model.NewPassword))
                {
                    return View("Error");
                }

                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), new { Email = account.Email });
            }
            else if (ModelState.ContainsKey(nameof(ResetPasswordViewModel.NewPassword)))
            {
                ModelState.Remove(nameof(ResetPasswordViewModel.NewPassword));
                ModelState.AddModelError(nameof(ResetPasswordViewModel.NewPassword), _stringOptions.Password_Invalid);
            }

            return View(model);
        }

        /// <summary>
        /// GET: /Account/ResetPasswordConfirmation
        /// </summary>
        /// <returns>
        /// ResetPasswordConfirmation view.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation(string email)
        {
            return View(model: email);
        }

        /// <summary>
        /// GET: /Account/EmailConfirmation
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// EmailVerificationConfirmation view and sets account's email confirmed to true if token, email and model state are valid.
        /// Error view if token or email or model state are invalid.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        [SetSignedInAccount]
        public async Task<IActionResult> EmailVerificationConfirmation(EmailVerificationConfirmationViewModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _vakAccountRepository.GetAccountAsync(model.AccountId);

                if(account != null &&
                    await _accountSecurityServices.ValidateTokenAsync(TokenServiceOptions.DataProtectionTokenService, _confirmEmailPurpose, account, model.Token) &&
                    await _vakAccountRepository.UpdateAccountEmailConfirmedAsync(account.AccountId))
                {
                    return View(model: account.Email);
                }
            }

            return View("Error");
        }

        /// <summary>
        /// Get: /Account/ManageAccount
        /// </summary>
        /// <returns>
        /// ManageAccount view if authentication succeeds.
        /// Redirects to /Account/Login if authentication fails.
        /// </returns>
        [HttpGet]
        [SetSignedInAccount]
        public IActionResult ManageAccount()
        {
            return View();
        }

        [HttpGet]
        [SetSignedInAccount]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangeEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeEmail(ChangeEmailViewModel model)
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangeAlternativeEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeAlternativeEmail(ChangeAlternativeEmailViewModel model)
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangeDisplayName()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeDisplayName(ChangeDisplayNameViewModel model)
        {
            return View();
        }

        [HttpGet]
        public IActionResult SendEmailVerificationConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DisableTwoFactorConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult EnableTwoFactor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnableTwoFactor(EnableTwoFactorViewModel model)
        {
            return View();
        }

        [HttpGet]
        public IActionResult EnableTwoFactorConfirmation()
        {
            return View();
        }

        #region Helpers

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
