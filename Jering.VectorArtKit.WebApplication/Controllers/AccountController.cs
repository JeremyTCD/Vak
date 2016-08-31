using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jering.VectorArtKit.WebApplication.ViewModels;
using Jering.AccountManagement.DatabaseInterface;
using Microsoft.AspNetCore.Http.Authentication;
using Jering.AccountManagement.Security;
using Jering.VectorArtKit.WebApplication.BusinessModel;
using Jering.VectorArtKit.WebApplication.Filters;
using Microsoft.Extensions.Options;
using Jering.VectorArtKit.WebApplication.ViewModels.Shared;
using Jering.VectorArtKit.WebApplication.ViewModels.Account;

namespace Jering.VectorArtKit.WebApplication.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private VakAccountRepository _vakAccountRepository;
        private IAccountSecurityServices<VakAccount> _accountSecurityServices;
        private StringOptions _stringOptions;
        private string _confirmEmailPurpose = "ConfirmEmail";
        private string _twoFactorPurpose = "TwoFactor";

        public AccountController(
            IAccountRepository<VakAccount> vakAccountRepository,
            IAccountSecurityServices<VakAccount> accountSecurityServices,
            IOptions<StringOptions> viewModelOptionsAccessor
            )
        {
            _vakAccountRepository = (VakAccountRepository)vakAccountRepository;
            _accountSecurityServices = accountSecurityServices;
            _stringOptions = viewModelOptionsAccessor?.Value;
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
                        nameof(AccountController.ConfirmEmail), 
                        nameof(AccountController).Replace("Controller", ""), 
                        new ConfirmEmailViewModel{ Token = token }, 
                        protocol: HttpContext.Request.Scheme);

                    await _accountSecurityServices.SendEmailAsync(model.Email, _stringOptions.ConfirmEmail_Subject, string.Format(_stringOptions.ConfirmEmail_Message, callbackUrl));

                    // TODO: this should eventually redirect to account vakkits page
                    return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", ""));
                }

                ModelState.AddModelError(nameof(SignUpViewModel.Email), _stringOptions.SignUp_AccountWithEmailExists);
            }

            return View(model);
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
                    return RedirectToAction(nameof(VerifyTwoFactorCode), new { IsPersistent = model.RememberMe, ReturnUrl = returnUrl });
                }
                if (passwordSignInResult == PasswordSignInResult.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
            }

            ModelState.AddModelError(string.Empty, _stringOptions.Login_Failed);
            return View(model);
        }

        //
        // GET: /Account/VerifyTwoFactorCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyTwoFactorCode(bool rememberMe, string returnUrl = null)
        {            
            VakAccount vakAccount = await _accountSecurityServices.GetTwoFactorAccountAsync();
            if (vakAccount == null)
            {
                return View("Error");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View(new VerifyCodeViewModel { IsPersistent = rememberMe});
        }

        //
        // POST: /Account/VerifyTwoFactorCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyTwoFactorCode(VerifyCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TwoFactorSignInResult twoFactorSignInResult = await _accountSecurityServices.TwoFactorSignInAsync(model.Code, model.IsPersistent);

            if (twoFactorSignInResult == TwoFactorSignInResult.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError(nameof(VerifyCodeViewModel.Code), _stringOptions.VerifyTwoFactorCode_InvalidCode);
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
        /// POST: /Account/ResendConfirmationEmail
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public IActionResult ResendConfirmationEmail(int accountId)
        {
            //TODO: resend confirmation email and return okay response

            return Ok();
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailViewModel model)
        {
            ConfirmEmailResult emailConfirmationResult = await _accountSecurityServices.ConfirmEmailAsync(model.Token);

            if (emailConfirmationResult == ConfirmEmailResult.Succeeded)
            {
                return View();
            }
            if(emailConfirmationResult == ConfirmEmailResult.InvalidToken)
            {

            }
            if(emailConfirmationResult == ConfirmEmailResult.NotLoggedIn)
            {

            }

            return View("Error");
        }

        /// <summary>
        /// GET: /Account/ForgotPassword
        /// </summary>
        /// <returns></returns>
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
                if (account != null)
                {                
                    // TODO: generate email and use sendEmailAsync to send it
                    //await _accountSecurityServices.SendConfirmationEmailAsync(account);
                }                
            }

            return View(nameof(ForgotPasswordConfirmation));
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
