using Jering.AccountManagement.DatabaseInterface;
using Jering.AccountManagement.Security;
using Jering.Mail;
using Jering.VectorArtKit.WebApi.BusinessModels;
using Jering.VectorArtKit.WebApi.Filters;
using Jering.VectorArtKit.WebApi.FormModels;
using Jering.VectorArtKit.WebApi.Options;
using Jering.VectorArtKit.WebApi.Resources;
using Jering.VectorArtKit.WebApi.ResponseModels.Account;
using Jering.VectorArtKit.WebApi.ResponseModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private IAccountSecurityService<VakAccount> _accountSecurityService;
        private IAccountRepository<VakAccount> _vakAccountRepository;
        private IEmailService _emailService;
        private UrlOptions _urlOptions;
        private string _confirmEmailPurpose = "ConfirmEmail";
        private string _confirmAlternativeEmailPurpose = "ConfirmAlternativeEmail";
        private string _resetPasswordPurpose = "ResetPassword";
        private string _twoFactorPurpose = "TwoFactor";

        public AccountController(
            IAccountRepository<VakAccount> vakAccountRepository,
            IAccountSecurityService<VakAccount> accountSecurityService,
            IEmailService emailService,
            IOptions<UrlOptions> urlOptionsWrapper
            )
        {
            _vakAccountRepository = vakAccountRepository;
            _accountSecurityService = accountSecurityService;
            _emailService = emailService;
            _urlOptions = urlOptionsWrapper.Value;
        }

        /// <summary>
        /// POST: /Account/SignUp
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest with <see cref="ErrorResponseModel"/>  if anti-forgery credentials are invalid.
        /// 400 BadRequest with <see cref="ErrorResponseModel"/> if model state is invalid. 
        /// 400 BadRequest with <see cref="ErrorResponseModel"/> if email in use.
        /// 200 OK with <see cref="SignUpResponseModel"/>, application cookie and sends email verification email if account is created successfully.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([FromBody] SignUpFormModel model)
        {
            if (ModelState.IsValid)
            {
                CreateAccountResult<VakAccount> createAccountResult = await _accountSecurityService.CreateAccountAsync(model.Email, model.Password);

                if (createAccountResult.Succeeded)
                {
                    await SendEmailVerificationEmail(createAccountResult.Account);

                    return Ok(new SignUpResponseModel() { Username = createAccountResult.Account.Email });
                }

                ModelState.AddModelError(nameof(SignUpFormModel.Email), Strings.ErrorMessage_Email_InUse);
            }

            return BadRequest(new ErrorResponseModel()
            {
                ExpectedError = true,
                ModelState = new SerializableError(ModelState)
            });
        }

        /// <summary>
        /// Post: /Account/LogIn
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns>
        /// 400 BadRequest with <see cref="ErrorResponseModel"/>  if anti-forgery credentials are invalid.
        /// 400 BadRequest with <see cref="ErrorResponseModel"> if model state is invalid. 
        /// 400 BadRequest with <see cref="ErrorResponseModel"> if credentials are invalid. 
        /// 200 OK with <see cref="LogInResponseModel"/>  and application cookie if login succeeds.
        /// 200 OK with <see cref="LogInResponseModel"/>, two factor cookie and sends two factor email if two factor authentication is required. 
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn([FromBody] LogInFormModel model)
        {
            if (ModelState.IsValid)
            {
                PasswordLogInResult<VakAccount> result = await _accountSecurityService.PasswordLogInAsync(model.Email,
                    model.Password,
                    new AuthenticationProperties() { IsPersistent = model.RememberMe });

                if (result.TwoFactorRequired)
                {
                    await _accountSecurityService.CreateTwoFactorCookieAsync(result.Account);
                    await SendTwoFactorEmail(result.Account);

                    return Ok(new LogInResponseModel() {
                        TwoFactorRequired = true,
                        IsPersistent = model.RememberMe
                    });
                }
                if (result.Succeeded)
                {
                    return Ok(new LogInResponseModel() {
                        TwoFactorRequired = false,
                        Username = model.Email,
                        IsPersistent = model.RememberMe
                    });
                }

                return BadRequest(new ErrorResponseModel()
                {
                    ExpectedError = true,
                    ErrorMessage = Strings.ErrorMessage_LogIn_Failed
                });
            }

            return BadRequest(new ErrorResponseModel()
            {
                ExpectedError = true,
                ModelState = new SerializableError(ModelState)
            });
        }

        /// <summary>
        /// Get: /Account/LogOff
        /// Note that this action is not protected from CSRF. Navigating to https://accounts.google.com/Logout 
        /// logs you out of all google accounts. This means google does not consider logout CSRF to be a threat.
        /// </summary>
        /// <returns>
        /// 401 Unauthorized with <see cref="ErrorResponseModel>"/>  if authentication fails.
        /// 200 OK with two factor cookie and application cookie (with empty string values) if authentication succeeds. 
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
            await _accountSecurityService.LogOffAsync();
            return Ok();
        }

        /// <summary>
        /// POST: /Account/TwoFactorLogIn
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest with <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 400 BadRequest with <see cref="ErrorResponseModel"> if model state is invalid. 
        /// 400 BadRequest with <see cref="ErrorResponseModel"> if code is invalid. 
        /// 200 OK with <see cref="TwoFactorLogInResponseModel"/>, two factor cookie and application cookie if login succeeds.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TwoFactorLogIn([FromBody] TwoFactorLogInFormModel model)
        {
            if (ModelState.IsValid)
            {
                TwoFactorLogInResult<VakAccount> result = await _accountSecurityService.TwoFactorLogInAsync(model.Code, model.IsPersistent);

                if (result.Succeeded)
                {
                    return Ok(new TwoFactorLogInResponseModel()
                    {
                        Username = result.Account.Email,
                        IsPersistent = model.IsPersistent
                    });
                }

                ModelState.AddModelError(nameof(TwoFactorLogInFormModel.Code), Strings.ErrorMessage_TwoFactorCode_Invalid);
            }

            return BadRequest(new ErrorResponseModel()
            {
                ExpectedError = true,
                ModelState = new SerializableError(ModelState)
            });
        }

        /// <summary>
        /// POST: /Account/SendResetPasswordEmail
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest with <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 400 BadRequest with <see cref="ErrorResponseModel"> if model state is invalid. 
        /// 400 BadRequest with <see cref="ErrorResponseModel"> if email is invalid. 
        /// 200 OK and sends reset password email if email is valid.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendResetPasswordEmail([FromBody] SendResetPasswordEmailFormModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _vakAccountRepository.GetAccountByEmailOrAlternativeEmailAsync(model.Email);
                if (account != null)
                {
                    await SendResetPasswordEmail(account);

                    return Ok();
                }

                ModelState.AddModelError(nameof(SendResetPasswordEmailFormModel.Email), Strings.ErrorMessage_Email_Invalid);
            }

            return BadRequest(new ErrorResponseModel()
            {
                ExpectedError = true,
                ModelState = new SerializableError(ModelState)
            });
        }

        ///// <summary>
        ///// GET: /Account/ResetPassword
        ///// </summary>
        ///// <returns>
        ///// ResetPassword view with anti-forgery token and cookie.
        ///// </returns>
        //[HttpGet]
        //[AllowAnonymous]
        //public IActionResult ResetPassword(string token, string email)
        //{
        //    if (token == null || email == null)
        //    {
        //        return View("Error");
        //    }

        //    return View(new ResetPasswordFormModel { Email = email });
        //}

        ///// <summary>
        ///// POST: /Account/ResetPassword
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns>
        ///// ResetPassword view with error message if model state is invalid.
        ///// Redirects to ResetPasswordConfirmation view and updates password if email, token and model state are valid.
        ///// Error view if token or email is invalid.
        ///// BadRequest if anti-forgery credentials are invalid.
        ///// </returns>
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ResetPassword(ResetPasswordFormModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        VakAccount account = model.Email == null ? null : await _vakAccountRepository.GetAccountByEmailOrAlternativeEmailAsync(model.Email);
        //        if (account == null || model.Token == null ||
        //            !await _accountSecurityService.ValidateTokenAsync(TokenServiceOptions.DataProtectionTokenService, _resetPasswordPurpose, account, model.Token) ||
        //            !await _vakAccountRepository.UpdateAccountPasswordHashAsync(account.AccountId, model.NewPassword))
        //        {
        //            return View("Error");
        //        }

        //        return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), new { Email = account.Email });
        //    }
        //    else if (ModelState.ContainsKey(nameof(ResetPasswordFormModel.NewPassword)))
        //    {
        //        ModelState.Remove(nameof(ResetPasswordFormModel.NewPassword));
        //        // Broke the following error message up 
        //        //ModelState.AddModelError(nameof(ResetPasswordFormModel.NewPassword), Strings.ErrorMessage_Password_FormatInvalid);
        //    }

        //    return View(model);
        //}

        ///// <summary>
        ///// GET: /Account/ResetPasswordConfirmation
        ///// </summary>
        ///// <returns>
        ///// ResetPasswordConfirmation view.
        ///// </returns>
        //[HttpGet]
        //[AllowAnonymous]
        //public IActionResult ResetPasswordConfirmation(string email)
        //{
        //    return View(model: email);
        //}

        ///// <summary>
        ///// Get: /Account/ManageAccount
        ///// </summary>
        ///// <returns>
        ///// ManageAccount view if authentication succeeds.
        ///// Redirects to /Account/Login if authentication fails.
        ///// </returns>
        //[HttpGet]
        //[SetSignedInAccount]
        //public IActionResult ManageAccount()
        //{
        //    return View();
        //}

        ///// <summary>
        ///// Get: /Account/ChangePassword
        ///// </summary>
        ///// <returns>
        ///// ChangePassword view with anti-forgery token and cookie if authentication succeeds.
        ///// Redirects to /Account/Login if authentication fails.
        ///// </returns>
        //[HttpGet]
        //[SetSignedInAccount]
        //public IActionResult ChangePassword()
        //{
        //    return View();
        //}

        ///// <summary>
        ///// Post: /Account/ChangePassword
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns>
        ///// Error view if unable to update database.
        ///// ChangePassword view with error messages if model state is invalid or current password is invalid.
        ///// Redirects to /Account/ManageAccount with a new application cookie and updates password hash if successful.
        ///// BadRequest if anti-forgery credentials are invalid.
        ///// Redirects to /Account/Login if authentication fails.
        ///// </returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[SetSignedInAccount]
        //public async Task<IActionResult> ChangePassword(ChangePasswordFormModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string email = _accountSecurityService.GetSignedInAccountEmail();
        //        VakAccount account = await _vakAccountRepository.GetAccountByEmailAndPasswordAsync(email, model.CurrentPassword);
        //        if (account == null)
        //        {
        //            ModelState.AddModelError(nameof(ChangePasswordFormModel.CurrentPassword), Strings.ErrorMessage_CurrentPassword_Invalid);
        //        }
        //        else
        //        {
        //            UpdateAccountPasswordHashResult result = await _accountSecurityService.UpdateAccountPasswordHashAsync(account.AccountId, model.NewPassword);

        //            if (result.Failed)
        //            {
        //                return View("Error");
        //            }

        //            account = await _vakAccountRepository.GetAccountAsync(account.AccountId);
        //            await _accountSecurityService.RefreshSignInAsync(account);

        //            return RedirectToAction(nameof(AccountController.ManageAccount));
        //        }
        //    }
        //    else if (ModelState.ContainsKey(nameof(ChangePasswordFormModel.NewPassword)))
        //    {
        //        ModelState.Remove(nameof(ChangePasswordFormModel.NewPassword));
        //        ModelState.AddModelError(nameof(ChangePasswordFormModel.NewPassword), Strings.ErrorMessage_NewPassword_FormatInvalid);
        //    }

        //    return View(model);
        //}

        ///// <summary>
        ///// Get: /Account/ChangeEmail
        ///// </summary>
        ///// <returns>
        ///// ChangeEmail view with anti-forgery token and cookie if authentication succeeds.
        ///// Redirects to /Account/Login if authentication fails.
        ///// </returns>
        //[HttpGet]
        //[SetSignedInAccount]
        //public IActionResult ChangeEmail()
        //{
        //    return View();
        //}

        ///// <summary>
        ///// Post: /Account/ChangeEmail
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns>
        ///// Error view if unable to update database or new email is identical to current email.
        ///// ChangeEmail view with error messages if model state is invalid, password is invalid, email is in use.
        ///// Redirects to /Account/ManageAccount with a new application cookie and updates email if successful.
        ///// BadRequest if anti-forgery credentials are invalid.
        ///// Redirects to /Account/Login if authentication fails.
        ///// </returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[SetSignedInAccount]
        //public async Task<IActionResult> ChangeEmail(ChangeEmailFormModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string currentEmail = _accountSecurityService.GetSignedInAccountEmail();
        //        if (currentEmail == model.NewEmail)
        //        {
        //            // If we get here something has gone wrong since model validation should ensure that current email and new email differ
        //            return View("Error");
        //        }
        //        VakAccount account = await _vakAccountRepository.GetAccountByEmailAndPasswordAsync(currentEmail, model.Password);
        //        if (account == null)
        //        {
        //            ModelState.AddModelError(nameof(ChangeEmailFormModel.Password), Strings.ErrorMessage_Password_Invalid);
        //        }
        //        else
        //        {
        //            UpdateAccountEmailResult result = await _accountSecurityService.UpdateAccountEmailAsync(account.AccountId, model.NewEmail);

        //            if (result.Failed)
        //            {
        //                return View("Error");
        //            }

        //            if (result.EmailInUse)
        //            {
        //                ModelState.AddModelError(nameof(ChangeEmailFormModel.NewEmail), Strings.ErrorMessage_EmailInUse);
        //            }
        //            else
        //            {
        //                account = await _vakAccountRepository.GetAccountAsync(account.AccountId);
        //                await _accountSecurityService.RefreshSignInAsync(account);

        //                return RedirectToAction(nameof(AccountController.ManageAccount));
        //            }
        //        }
        //    }

        //    return View(model);
        //}

        ///// <summary>
        ///// Get: /Account/ChangeAlternativeEmail
        ///// </summary>
        ///// <returns>
        ///// ChangeAlternativeEmail view with anti-forgery token and cookie if authentication succeeds.
        ///// Redirects to /Account/Login if authentication fails.
        ///// </returns>
        //[HttpGet]
        //[SetSignedInAccount]
        //public IActionResult ChangeAlternativeEmail()
        //{
        //    return View();
        //}

        ///// <summary>
        ///// Post: /Account/ChangeAlternativeEmail
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns>
        ///// Error view if unable to update database or new alternative email is identical to current alternative email.
        ///// ChangeAlternativeEmail view with error messages if model state is invalid, password is invalid or alternative email is in use.
        ///// Redirects to /Account/ManageAccount and updates alternative email if successful.
        ///// BadRequest if anti-forgery credentials are invalid.
        ///// Redirects to /Account/Login if authentication fails.
        ///// </returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[SetSignedInAccount]
        //public async Task<IActionResult> ChangeAlternativeEmail(ChangeAlternativeEmailFormModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string currentEmail = _accountSecurityService.GetSignedInAccountEmail();
        //        VakAccount account = await _vakAccountRepository.GetAccountByEmailAndPasswordAsync(currentEmail, model.Password);

        //        if (account == null)
        //        {
        //            ModelState.AddModelError(nameof(ChangeAlternativeEmailFormModel.Password), Strings.ErrorMessage_Password_Invalid);
        //        }
        //        else if (account.AlternativeEmail == model.NewAlternativeEmail)
        //        {
        //            // If we get here something has gone wrong since model validation should ensure that current and new alternative emails differ
        //            return View("Error");
        //        }
        //        else
        //        {
        //            UpdateAccountAlternativeEmailResult result = await _accountSecurityService.UpdateAccountAlternativeEmailAsync(account.AccountId, model.NewAlternativeEmail);

        //            if (result.Failed)
        //            {
        //                return View("Error");
        //            }

        //            if (result.AlternativeEmailInUse)
        //            {
        //                ModelState.AddModelError(nameof(ChangeAlternativeEmailFormModel.NewAlternativeEmail), Strings.ErrorMessage_EmailInUse);
        //            }
        //            else
        //            {
        //                return RedirectToAction(nameof(AccountController.ManageAccount));
        //            }
        //        }
        //    }

        //    return View(model);
        //}

        ///// <summary>
        ///// Get: /Account/ChangeDisplayName
        ///// </summary>
        ///// <returns>
        ///// ChangeDisplayName view with anti-forgery token and cookie if authentication succeeds.
        ///// Redirects to /Account/Login if authentication fails.
        ///// </returns>
        //[HttpGet]
        //[SetSignedInAccount]
        //public IActionResult ChangeDisplayName()
        //{
        //    return View();
        //}

        ///// <summary>
        ///// Post: /Account/ChangeDisplayName
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns>
        ///// Error view if unable to update database or new display name is identical to current display name.
        ///// ChangeDisplayName view with error messages if model state is invalid, password is invalid or display name is in use.
        ///// Redirects to /Account/ManageAccount and updates display name if successful.
        ///// BadRequest if anti-forgery credentials are invalid.
        ///// Redirects to /Account/Login if authentication fails.
        ///// </returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[SetSignedInAccount]
        //public async Task<IActionResult> ChangeDisplayName(ChangeDisplayNameFormModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string currentEmail = _accountSecurityService.GetSignedInAccountEmail();
        //        VakAccount account = await _vakAccountRepository.GetAccountByEmailAndPasswordAsync(currentEmail, model.Password);

        //        if (account == null)
        //        {
        //            ModelState.AddModelError(nameof(ChangeAlternativeEmailFormModel.Password), Strings.ErrorMessage_Password_Invalid);
        //        }
        //        else if (account.DisplayName == model.NewDisplayName)
        //        {
        //            // If we get here something has gone wrong since model validation should ensure that current and new display names differ
        //            return View("Error");
        //        }
        //        else
        //        {
        //            UpdateAccountDisplayNameResult result = await _accountSecurityService.UpdateAccountDisplayNameAsync(account.AccountId, model.NewDisplayName);

        //            if (result.Failed)
        //            {
        //                return View("Error");
        //            }

        //            if (result.DisplayNameInUse)
        //            {
        //                ModelState.AddModelError(nameof(ChangeDisplayNameFormModel.NewDisplayName), Strings.ErrorMessage_DisplayName_InUse);
        //            }
        //            else
        //            {
        //                return RedirectToAction(nameof(AccountController.ManageAccount));
        //            }
        //        }
        //    }

        //    return View(model);
        //}

        ///// <summary>
        ///// Post: /Account/EnableTwoFactor
        ///// </summary>
        ///// <returns>
        ///// Redirects to /Account/ManageAccount view if two factor already enabled or is successfully enabled.
        ///// Redirects to /Account/TestTwoFactorCode view and sends two factor email if email is not verified.
        ///// Error view if unable to retrieve signed in account or unable to update two factor enabled.
        ///// Redirects to /Account/Login if authentication fails.
        ///// BadRequest if anti-forgery credentials are invalid.
        ///// </returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[SetSignedInAccount]
        //public async Task<IActionResult> EnableTwoFactor()
        //{
        //    VakAccount account = await _accountSecurityService.GetSignedInAccountAsync();

        //    if (account == null)
        //    {
        //        return View("Error");
        //    }

        //    if (account.TwoFactorEnabled)
        //    {
        //        return RedirectToAction(nameof(ManageAccount));
        //    }

        //    if (account.EmailVerified)
        //    {
        //        if((await _accountSecurityService.UpdateAccountTwoFactorEnabledAsync(account.AccountId, true)).Failed){
        //            return View("Error");
        //        }

        //        return RedirectToAction(nameof(ManageAccount));
        //    }

        //    await SendTwoFactorEmail(account);

        //    return RedirectToAction(nameof(TestTwoFactor));
        //}

        ///// <summary>
        ///// Post: /Account/EnableTwoFactor
        ///// </summary>
        ///// <returns>
        ///// Redirects to /Account/ManageAccount view if two factor already disable or is successfully disabled.
        ///// Error view if unable to retrieve signed in account or unable to update two factor enabled.
        ///// Redirects to /Account/Login if authentication fails.
        ///// BadRequest if anti-forgery credentials are invalid.
        ///// </returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[SetSignedInAccount]
        //public async Task<IActionResult> DisableTwoFactor()
        //{
        //    VakAccount account = await _accountSecurityService.GetSignedInAccountAsync();

        //    if (account == null)
        //    {
        //        return View("Error");
        //    }

        //    if (!account.TwoFactorEnabled)
        //    {
        //        return RedirectToAction(nameof(ManageAccount));
        //    }

        //    if ((await _accountSecurityService.UpdateAccountTwoFactorEnabledAsync(account.AccountId, false)).Failed)
        //    {
        //        return View("Error");
        //    }

        //    return RedirectToAction(nameof(ManageAccount));
        //}

        ///// <summary>
        ///// Get: /Account/TestTwoFactor
        ///// </summary>
        ///// <returns>
        ///// TestTwoFactor view with anti-forgery credentials if authentication succeeds.
        ///// Redirects to /Account/Login if authentication fails.
        ///// </returns>
        //[HttpGet]
        //[SetSignedInAccount]
        //public IActionResult TestTwoFactor()
        //{
        //    return View();
        //}

        ///// <summary>
        ///// Post: /Account/TestTwoFactor
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns>
        ///// TestTwoFactor view with error messages if model state is invalid.
        ///// TestTwoFactor view with error message if token is invalid.
        ///// Error view if unable to retrieve signed in account, unable to update two factor enabled or unable to update email verified.
        ///// Redirects to /Account/ManageAccount, updates two factor enabled and updates email verified if successful.
        ///// BadRequest if anti-forgery credentials are invalid.
        ///// Redirects to /Account/Login if authentication fails.
        ///// </returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[SetSignedInAccount]
        //public async Task<IActionResult> TestTwoFactor(TestTwoFactorFormModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        VakAccount account = await _accountSecurityService.GetSignedInAccountAsync();

        //        if (account == null)
        //        {
        //            return View("Error");
        //        }

        //        if (await _accountSecurityService.ValidateTokenAsync(TokenServiceOptions.TotpTokenService, _twoFactorPurpose, account, model.Code))
        //        {
        //            // wrap UpdateAccountEmailVerifiedAsync in ass for consistency? 
        //            if (!account.EmailVerified && !await _vakAccountRepository.UpdateAccountEmailVerifiedAsync(account.AccountId, true) ||
        //                !account.TwoFactorEnabled && (await _accountSecurityService.UpdateAccountTwoFactorEnabledAsync(account.AccountId, true)).Failed)
        //            {
        //                return View("Error");
        //            }

        //            return RedirectToAction(nameof(AccountController.ManageAccount));
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(nameof(TestTwoFactorFormModel.Code), Strings.ErrorMessage_TwoFactorCode_Invalid);
        //        }
        //    }

        //    return View(model);
        //}

        ///// <summary>
        ///// Post: /Account/SendEmailVerificationEmail
        ///// </summary>
        ///// <returns>
        ///// Redirects to /Account/SendEmailVerificationEmailConfirmation and sends email verification email if successful.
        ///// Error view if unable to retrieve signed in account or email is already verified.
        ///// Redirects to /Account/Login if authentication fails.
        ///// BadRequest if anti-forgery credentials are invalid.
        ///// </returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[SetSignedInAccount]
        //public async Task<IActionResult> SendEmailVerificationEmail()
        //{
        //    VakAccount account = await _accountSecurityService.GetSignedInAccountAsync();

        //    if (account == null || account.EmailVerified)
        //    {
        //        return View("Error");
        //    }

        //    await SendEmailVerificationEmail(account);

        //    return RedirectToAction(nameof(SendEmailVerificationEmailConfirmation), new { Email = account.Email });
        //}

        ///// <summary>
        ///// Post: /Account/SendAlternativeEmailVerificationEmail
        ///// </summary>
        ///// <returns>
        ///// Redirects to /Account/SendAlternativeEmailVerificationEmailConfirmation and sends verification email if successful.
        ///// Error view if unable to retrieve signed in account or alternative email is already verified or alternative view is null.
        ///// Redirects to /Account/Login if authentication fails.
        ///// BadRequest if anti-forgery credentials are invalid.
        ///// </returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[SetSignedInAccount]
        //public async Task<IActionResult> SendAlternativeEmailVerificationEmail()
        //{
        //    VakAccount account = await _accountSecurityService.GetSignedInAccountAsync();

        //    if (account == null || account.AlternativeEmail == null || account.AlternativeEmailVerified)
        //    {
        //        return View("Error");
        //    }

        //    await SendAlternativeEmailVerificationEmail(account);

        //    return RedirectToAction(nameof(SendEmailVerificationEmailConfirmation));
        //}

        ///// <summary>
        ///// Get: /Account/SendEmailVerificationEmailConfirmation
        ///// </summary>
        ///// <returns>
        ///// SendEmailVerificationEmailConfirmation view if authentication succeeds.
        ///// Redirects to /Account/Login if authentication fails.
        ///// </returns>
        //[HttpGet]
        //public IActionResult SendEmailVerificationEmailConfirmation(string email)
        //{
        //    return View();
        //}

        /// <summary>
        /// GET: /Account/EmailVerificationConfirmation
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// EmailVerificationConfirmation view and sets account's email confirmed to true if token, email and model state are valid.
        /// Error view if token or email or model state are invalid.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> EmailVerificationConfirmation(EmailVerificationConfirmationFormModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _vakAccountRepository.GetAccountAsync(model.AccountId);

                if (account != null &&
                    await _accountSecurityService.ValidateTokenAsync(TokenServiceOptions.DataProtectionTokenService, _confirmEmailPurpose, account, model.Token) &&
                    await _vakAccountRepository.UpdateAccountEmailVerifiedAsync(account.AccountId, true))
                {
                    // TODO redirect to confirmation view
                    return Ok();
                }
            }

            return BadRequest();
        }

        /// <summary>
        /// GET: /Account/AlternativeEmailVerificationConfirmation
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// AlternativeEmailVerificationConfirmation view and sets account's alternative email verified to true if token, email and model state are valid.
        /// Error view if token or email or model state are invalid.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AlternativeEmailVerificationConfirmation(AlternativeEmailVerificationConfirmationFormModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _vakAccountRepository.GetAccountAsync(model.AccountId);

                if (account != null &&
                    await _accountSecurityService.ValidateTokenAsync(TokenServiceOptions.DataProtectionTokenService, _confirmAlternativeEmailPurpose, account, model.Token) &&
                    await _vakAccountRepository.UpdateAccountAlternativeEmailVerifiedAsync(account.AccountId, true))
                {
                    // TODO redirect to confirmation view
                    return Ok();
                }
            }

            return BadRequest();
        }

        #region Helpers
        [NonAction]
        private async Task SendResetPasswordEmail(VakAccount account)
        {
            string token = await _accountSecurityService.GetTokenAsync(TokenServiceOptions.DataProtectionTokenService, _resetPasswordPurpose, account);

            MimeMessage mimeMessage = _emailService.CreateMimeMessage(account.Email,
                Strings.ResetPasswordEmail_Subject,
                string.Format(Strings.ResetPasswordEmail_Message,
                $"{ _urlOptions.ClientUrl}resetpassword?Token={token}&Email={account.Email}"));
            await _emailService.SendEmailAsync(mimeMessage);
        }

        // TODO url should not point to action
        [NonAction]
        private async Task SendEmailVerificationEmail(VakAccount account)
        {
            if(account.Email == null)
            {
                throw new ArgumentNullException();
            }            

            string token = await _accountSecurityService.GetTokenAsync(TokenServiceOptions.DataProtectionTokenService, _confirmEmailPurpose, account);
            string callbackUrl = Url.Action(
                nameof(AccountController.EmailVerificationConfirmation),
                nameof(AccountController).Replace("Controller", ""),
                new { Token = token, AccountId = account.AccountId },
                protocol: HttpContext.Request.Scheme);

            MimeMessage mimeMessage = _emailService.CreateMimeMessage(account.Email, 
                Strings.Email_EmailVerification_Subject, 
                string.Format(Strings.Email_EmailVerification_Message, 
                callbackUrl));
            await _emailService.SendEmailAsync(mimeMessage);
        }

        // TODO url should not point to action
        [NonAction]
        private async Task SendAlternativeEmailVerificationEmail(VakAccount account)
        {
            if (account.AlternativeEmail == null)
            {
                throw new ArgumentNullException();
            }

            string token = await _accountSecurityService.GetTokenAsync(TokenServiceOptions.DataProtectionTokenService,_confirmAlternativeEmailPurpose, account);
            string callbackUrl = Url.Action(
                nameof(AccountController.AlternativeEmailVerificationConfirmation),
                nameof(AccountController).Replace("Controller", ""),
                new { Token = token, AccountId = account.AccountId },
                protocol: HttpContext.Request.Scheme);

            MimeMessage mimeMessage = _emailService.CreateMimeMessage(account.AlternativeEmail,
                Strings.Email_EmailVerification_Subject,
                string.Format(Strings.Email_EmailVerification_Message,
                callbackUrl));
            await _emailService.SendEmailAsync(mimeMessage);
        }

        // TODO url should not point to action
        [NonAction]
        private async Task SendTwoFactorEmail(VakAccount account)
        {
            string token = await _accountSecurityService.GetTokenAsync(TokenServiceOptions.TotpTokenService, _twoFactorPurpose, account);
            MimeMessage mimeMessage = _emailService.CreateMimeMessage(account.Email, Strings.TwoFactorEmail_Subject, string.Format(Strings.TwoFactorEmail_Message, token));
            await _emailService.SendEmailAsync(mimeMessage);
        }
        //[NonAction]
        //private IActionResult RedirectToLocal(string returnUrl)
        //{
        //    // This prevents open redirection attacks - http://www.asp.net/mvc/overview/security/preventing-open-redirection-attacks
        //    if (Url.IsLocalUrl(returnUrl))
        //    {
        //        return Redirect(returnUrl);
        //    }
        //    else
        //    {
        //        return RedirectToAction(nameof(HomeController.Index), "Home");
        //    }
        //}

        #endregion
    }
}
