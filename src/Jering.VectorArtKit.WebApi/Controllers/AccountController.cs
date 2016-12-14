using Jering.Accounts.DatabaseInterface;
using Jering.Security;
using Jering.Utilities;
using Jering.VectorArtKit.WebApi.BusinessModels;
using Jering.VectorArtKit.WebApi.FormModels;
using Jering.VectorArtKit.WebApi.Options;
using Jering.VectorArtKit.WebApi.Resources;
using Jering.VectorArtKit.WebApi.ResponseModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Jering.Accounts;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Jering.VectorArtKit.WebApi.Extensions;

namespace Jering.VectorArtKit.WebApi.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private IAccountsService<VakAccount> _accountsService;
        private IAccountRepository<VakAccount> _vakAccountRepository;
        private IAntiforgery _antiforgery;
        private UrlOptions _urlOptions;

        public AccountController(
            IAccountRepository<VakAccount> vakAccountRepository,
            IAccountsService<VakAccount> accountsService,
            IAntiforgery antiforgery,
            IOptions<UrlOptions> urlOptionsWrapper
            )
        {
            _vakAccountRepository = vakAccountRepository;
            _accountsService = accountsService;
            _urlOptions = urlOptionsWrapper.Value;
            _antiforgery = antiforgery;
        }
        #region Session
        /// <summary>
        /// Post: /Account/LogIn
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 400 BadRequest and <see cref="LogInResponseModel"> if model state is invalid. 
        /// 400 BadRequest and <see cref="LogInResponseModel"> if credentials are invalid. 
        /// 400 BadRequest, <see cref="LogInResponseModel"/>, two factor cookie and sends two factor email 
        /// if two factor auth is required. 
        /// 200 OK, <see cref="LogInResponseModel"/>, application cookie and antiforgery cookies if login succeeds.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn([FromBody] LogInFormModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _vakAccountRepository.GetAccountByEmailAsync(model.Email);
                if (account != null && _accountsService.ValidatePassword(account, model.Password))
                {
                    if (account.TwoFactorEnabled)
                    {
                        await _accountsService.TwoFactorLogInAsync(account);
                        await _accountsService.SendTwoFactorCodeEmailAsync(account,
                            Strings.Email_Subject_TwoFactorCode,
                            Strings.Email_Message_TwoFactorCode);

                        return BadRequest(new LogInResponseModel
                        {
                            ExpectedError = true,
                            TwoFactorRequired = true,
                            IsPersistent = model.RememberMe
                        });
                    }

                    await _accountsService.ApplicationLogInAsync(account, new AuthenticationProperties { IsPersistent = model.RememberMe });
                    _antiforgery.AddAntiforgeryCookies(HttpContext);

                    return Ok(new LogInResponseModel
                    {
                        TwoFactorRequired = false,
                        Username = model.Email,
                        IsPersistent = model.RememberMe
                    });
                }

                // Don't reveal whether email or password was invalid
                return BadRequest(new LogInResponseModel
                {
                    ExpectedError = true,
                    ErrorMessage = Strings.ErrorMessage_LogIn_Failed
                });
            }

            return BadRequest(new LogInResponseModel
            {
                ExpectedError = true,
                ModelState = new SerializableError(ModelState)
            });
        }

        /// <summary>
        /// Get: /Account/LogOff
        /// </summary>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 200 OK and two factor cookie and application cookie (with empty string values) if auth succeeds. 
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _accountsService.ApplicationLogOffAsync();
            return Ok();
        }

        /// <summary>
        /// POST: /Account/TwoFactorLogIn
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 400 BadRequest and <see cref="TwoFactorLogInResponseModel"/> if model state is invalid. 
        /// 400 BadRequest and <see cref="TwoFactorLogInResponseModel"/> if two factor credentials are invalid. 
        /// 400 BadRequest and <see cref="TwoFactorLogInResponseModel"/> if code is expired. 
        /// 400 BadRequest and <see cref="TwoFactorLogInResponseModel"/> if code is invalid. 
        /// 200 OK, <see cref="TwoFactorLogInResponseModel"/>, two factor cookie, application 
        /// cookie and antiforgery cookies if login succeeds.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TwoFactorLogIn([FromBody] TwoFactorLogInFormModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _accountsService.GetTwoFactorAccountAsync();

                if (account == null)
                {
                    return BadRequest(new TwoFactorLogInResponseModel
                    {
                        ExpectedError = true,
                        ExpiredCredentials = true
                    });
                }

                ValidateTokenResult result = _accountsService.ValidateToken(TokenServiceOptions.TotpTokenService, _accountsService.TwoFactorTokenPurpose, account, model.Code);

                if (result.Valid)
                {
                    // Cleanup two factor cookie
                    await _accountsService.TwoFactorLogOffAsync();
                    await _accountsService.ApplicationLogInAsync(account, new AuthenticationProperties() { IsPersistent = model.IsPersistent });

                    _antiforgery.AddAntiforgeryCookies(HttpContext);

                    return Ok(new TwoFactorLogInResponseModel
                    {
                        Username = account.Email,
                        IsPersistent = model.IsPersistent
                    });
                }

                if (result.Expired)
                {
                    return BadRequest(new TwoFactorLogInResponseModel
                    {
                        ExpectedError = true,
                        ExpiredToken = true
                    });
                }

                ModelState.AddModelError(nameof(TwoFactorLogInFormModel.Code), Strings.ErrorMessage_TwoFactorCode_Invalid);
            }

            return BadRequest(new TwoFactorLogInResponseModel
            {
                ExpectedError = true,
                ModelState = new SerializableError(ModelState)
            });
        }
        #endregion

        #region Account Management
        /// <summary>
        /// POST: /Account/SignUp
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 400 BadRequest and <see cref="SignUpResponseModel"/> if model state is invalid. 
        /// 400 BadRequest and <see cref="SignUpResponseModel"/> if email in use.
        /// 200 OK, <see cref="SignUpResponseModel"/>, application cookie, antiforgery cookies and sends email verification 
        /// email if account is created successfully.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([FromBody] SignUpFormModel model)
        {
            if (ModelState.IsValid)
            {
                CreateAccountResult<VakAccount> createAccountResult = await _accountsService.CreateAccountAsync(model.Email, model.Password);

                if (createAccountResult.Succeeded)
                {
                    await _accountsService.SendEmailVerificationEmailAsync(createAccountResult.Account,
                        Strings.Email_Subject_EmailVerification,
                        Strings.Email_Message_EmailVerification,
                        _urlOptions.ClientDomain);

                    await _accountsService.ApplicationLogInAsync(createAccountResult.Account, new AuthenticationProperties { IsPersistent = true });

                    _antiforgery.AddAntiforgeryCookies(HttpContext);

                    return Ok(new SignUpResponseModel { Username = createAccountResult.Account.Email });
                }

                ModelState.AddModelError(nameof(SignUpFormModel.Email), Strings.ErrorMessage_Email_InUse);
            }

            return BadRequest(new SignUpResponseModel
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
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 400 BadRequest and <see cref="SendResetPasswordEmailResponseModel"> if model state is invalid. 
        /// 400 BadRequest and <see cref="SendResetPasswordEmailResponseModel"> if email is invalid. 
        /// 200 OK and sends reset password email if email is valid.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendResetPasswordEmail([FromBody] SendResetPasswordEmailFormModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _vakAccountRepository.GetAccountByEmailOrAltEmailAsync(model.Email);
                if (account != null)
                {
                    await _accountsService.SendResetPasswordEmailAsync(account,
                        model.Email,
                        Strings.Email_Subject_ResetPassword,
                        Strings.Email_Message_ResetPassword,
                        _urlOptions.ClientDomain);

                    return Ok();
                }

                ModelState.AddModelError(nameof(SendResetPasswordEmailFormModel.Email), Strings.ErrorMessage_Email_Invalid);
            }

            return BadRequest(new SendResetPasswordEmailResponseModel
            {
                ExpectedError = true,
                ModelState = new SerializableError(ModelState)
            });
        }

        /// <summary>
        /// POST: /Account/ResetPassword
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 400 BadRequest and <see cref="ResetPasswordResponseModel"/> if model state is invalid. 
        /// 400 BadRequest and <see cref="ResetPasswordResponseModel"/> if email is invalid. 
        /// 400 BadRequest and <see cref="ResetPasswordResponseModel"/> if token is invalid or expired. 
        /// 400 BadRequest and <see cref="ResetPasswordResponseModel"/> if new password is same as current password. 
        /// 200 OK and <see cref="ResetPasswordResponseModel"/> if password reset succeeds.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordFormModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _vakAccountRepository.GetAccountByEmailAsync(model.Email);

                if (account == null)
                {
                    return BadRequest(new ResetPasswordResponseModel
                    {
                        ExpectedError = true,
                        InvalidEmail = true
                    });
                }

                ValidateTokenResult validateTokenResult = _accountsService.ValidateToken(TokenServiceOptions.DataProtectionTokenService,
                    _accountsService.ResetPasswordTokenPurpose,
                    account,
                    model.Token);

                if (!validateTokenResult.Valid)
                {
                    return BadRequest(new ResetPasswordResponseModel
                    {
                        ExpectedError = true,
                        InvalidToken = true
                    });
                }

                SetPasswordResult setPasswordResult = await _accountsService.SetPasswordAsync(account, model.NewPassword);

                if (setPasswordResult.Succeeded)
                {
                    return Ok(new ResetPasswordResponseModel
                    {
                        Email = model.Email
                    });
                }

                ModelState.AddModelError(nameof(SetPasswordFormModel.NewPassword), Strings.ErrorMessage_NewPassword_MustDiffer);
            }

            return BadRequest(new ResetPasswordResponseModel
            {
                ExpectedError = true,
                ModelState = new SerializableError(ModelState)
            });
        }

        /// <summary>
        /// Get: /Account/GetAccountDetails
        /// </summary>
        /// <returns>
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 200 OK and <see cref="GetAccountDetailsResponseModel"/> if auth succeeds.
        /// </returns>
        /// <exception cref="NullReferenceException">Thrown if unable to retrieve logged in account</exception>
        [HttpGet]
        public async Task<IActionResult> GetAccountDetails()
        {
            VakAccount account = await _accountsService.GetApplicationAccountAsync();

            if (account == null)
            {
                // Unexpected error - logged in but unable to retrieve account
                throw new NullReferenceException(nameof(account));
            }

            return Ok(new GetAccountDetailsResponseModel
            {
                AltEmail = account.AltEmail,
                AltEmailVerified = account.AltEmailVerified,
                DisplayName = account.DisplayName,
                DurationSinceLastPasswordChange = (DateTime.UtcNow - account.PasswordLastChanged).ToElapsedDurationString(),
                Email = account.Email,
                EmailVerified = account.EmailVerified,
                TwoFactorEnabled = account.TwoFactorEnabled
            });
        }

        /// <summary>
        /// Post: /Account/SetPassword
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 400 BadRequest and <see cref="SetPasswordResponseModel"/> if model state is invalid. 
        /// 400 BadRequest and <see cref="SetPasswordResponseModel"/> if current password is invalid. 
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 200 OK and application cookie if password change succeeds.
        /// </returns>
        /// <exception cref="Exception">Thrown if unable to retrieve logged in account</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordFormModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _accountsService.GetApplicationAccountAsync();
                if (account == null)
                {
                    // Unexpected error - logged in (got past authorize attribute) but unable to retrieve account
                    throw new Exception();
                }

                if (_accountsService.ValidatePassword(account, model.CurrentPassword))
                {
                    SetPasswordResult result = await _accountsService.SetPasswordAsync(account, model.NewPassword);

                    if (result.Succeeded)
                    {
                        // Security stamp changes if password changes
                        account = await _vakAccountRepository.GetAccountAsync(account.AccountId);
                        await _accountsService.RefreshApplicationLogInAsync(account);

                        return Ok();
                    }

                    // SetPasswordFormModel validates that NewPassword != CurrentPassword. The only way this line runs is if
                    // password changes after VerifyPassword - possible but very unlikely.
                    ModelState.AddModelError(nameof(SetPasswordFormModel.NewPassword), Strings.ErrorMessage_NewPassword_MustDiffer);
                }
                else
                {
                    ModelState.AddModelError(nameof(SetPasswordFormModel.CurrentPassword), Strings.ErrorMessage_Password_Invalid);
                }
            }

            return BadRequest(new SetPasswordResponseModel
            {
                ExpectedError = true,
                ModelState = new SerializableError(ModelState)
            });
        }

        /// <summary>
        /// Post: /Account/SetEmail
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 400 BadRequest and <see cref="SetEmailResponseModel"/> if model state is invalid. 
        /// 400 BadRequest and <see cref="SetEmailResponseModel"/> if password is invalid. 
        /// 400 BadRequest and <see cref="SetEmailResponseModel"/> if new email is in use. 
        /// 400 BadRequest and <see cref="SetEmailResponseModel"/> if new email is the same as current email. 
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 200 OK and application cookie if email change succeeds.
        /// </returns>
        /// <exception cref="Exception">Thrown if unable to retrieve logged in account</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetEmail([FromBody] SetEmailFormModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _accountsService.GetApplicationAccountAsync();
                if (account == null)
                {
                    // Unexpected error - logged in (got past authorize attribute) but unable to retrieve account
                    throw new Exception();
                }

                if (_accountsService.ValidatePassword(account, model.Password))
                {
                    SetEmailResult result = await _accountsService.SetEmailAsync(account, model.NewEmail);

                    if (result.Succeeded)
                    {
                        // Security stamp changes if email changes
                        account = await _vakAccountRepository.GetAccountAsync(account.AccountId);
                        await _accountsService.RefreshApplicationLogInAsync(account);

                        return Ok();
                    }

                    if (result.AlreadySet)
                    {
                        ModelState.AddModelError(nameof(SetEmailFormModel.NewEmail), Strings.ErrorMessage_NewEmail_MustDiffer);
                    }
                    else if (result.InvalidNewEmail)
                    {
                        ModelState.AddModelError(nameof(SetEmailFormModel.NewEmail), Strings.ErrorMessage_Email_InUse);
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(SetEmailFormModel.Password), Strings.ErrorMessage_Password_Invalid);
                }
            }

            return BadRequest(new SetEmailResponseModel
            {
                ExpectedError = true,
                ModelState = new SerializableError(ModelState)
            });
        }

        /// <summary>
        /// Post: /Account/SetAltEmail
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 400 BadRequest and <see cref="SetAltEmailResponseModel"/> if model state is invalid. 
        /// 400 BadRequest and <see cref="SetAltEmailResponseModel"/> if password is invalid. 
        /// 400 BadRequest and <see cref="SetAltEmailResponseModel"/> if new alternative email is in use. 
        /// 400 BadRequest and <see cref="SetAltEmailResponseModel"/> if new alternative email is the same 
        /// as current alternative email. 
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 200 OK if alternative email change succeeds.
        /// </returns>
        /// <exception cref="Exception">Thrown if unable to retrieve logged in account</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAltEmail([FromBody] SetAltEmailFormModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _accountsService.GetApplicationAccountAsync();
                if (account == null)
                {
                    // Unexpected error - logged in (got past authorize attribute) but unable to retrieve account
                    throw new Exception();
                }

                if (_accountsService.ValidatePassword(account, model.Password))
                {
                    SetAltEmailResult result = await _accountsService.SetAltEmailAsync(account, model.NewAltEmail);

                    if (result.Succeeded)
                    {
                        return Ok();
                    }

                    if (result.AlreadySet)
                    {
                        ModelState.AddModelError(nameof(SetAltEmailFormModel.NewAltEmail), Strings.ErrorMessage_NewEmail_MustDiffer);
                    }
                    else if (result.InvalidNewAltEmail)
                    {
                        ModelState.AddModelError(nameof(SetAltEmailFormModel.NewAltEmail), Strings.ErrorMessage_Email_InUse);
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(SetAltEmailFormModel.Password), Strings.ErrorMessage_Password_Invalid);
                }
            }

            return BadRequest(new SetAltEmailResponseModel
            {
                ExpectedError = true,
                ModelState = new SerializableError(ModelState)
            });
        }

        /// <summary>
        /// Post: /Account/SetDisplayName
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 400 BadRequest and <see cref="SetDisplayNameResponseModel"/> if model state is invalid. 
        /// 400 BadRequest and <see cref="SetDisplayNameResponseModel"/> if password is invalid. 
        /// 400 BadRequest and <see cref="SetDisplayNameResponseModel"/> if new display name is in use. 
        /// 400 BadRequest and <see cref="SetDisplayNameResponseModel"/> if new display name is the same 
        /// as current display name. 
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 200 OK if display name change succeeds.
        /// </returns>
        /// <exception cref="Exception">Thrown if unable to retrieve logged in account</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDisplayName([FromBody] SetDisplayNameFormModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _accountsService.GetApplicationAccountAsync();
                if (account == null)
                {
                    // Unexpected error - logged in (got past authorize attribute) but unable to retrieve account
                    throw new Exception();
                }

                if (_accountsService.ValidatePassword(account, model.Password))
                {
                    SetDisplayNameResult result = await _accountsService.SetDisplayNameAsync(account, model.NewDisplayName);

                    if (result.Succeeded)
                    {
                        return Ok();
                    }

                    if (result.AlreadySet)
                    {
                        ModelState.AddModelError(nameof(SetDisplayNameFormModel.NewDisplayName), Strings.ErrorMessage_NewDisplayName_MustDiffer);
                    }
                    else if (result.InvalidNewDisplayName)
                    {
                        ModelState.AddModelError(nameof(SetDisplayNameFormModel.NewDisplayName), Strings.ErrorMessage_DisplayName_InUse);
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(SetDisplayNameFormModel.Password), Strings.ErrorMessage_Password_Invalid);
                }
            }

            return BadRequest(new SetDisplayNameResponseModel
            {
                ExpectedError = true,
                ModelState = new SerializableError(ModelState)
            });
        }

        /// <summary>
        /// Post: /Account/SetTwoFactorEnabled
        /// </summary>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 400 BadRequest, <see cref="SetTwoFactorEnabledResponseModel"/> and sends two factor code email
        /// if account email is unverified.
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 200 OK if setting of two factor enabled succeeds.
        /// </returns>
        /// <exception cref="Exception">Thrown if unable to retrieve logged in account</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetTwoFactorEnabled([FromBody] SetTwoFactorEnabledFormModel model)
        {
            VakAccount account = await _accountsService.GetApplicationAccountAsync();

            if (account == null)
            {
                // Unexpected error - logged in (got past authorize attribute) but unable to retrieve account
                throw new Exception();
            }

            SetTwoFactorEnabledResult result = await _accountsService.SetTwoFactorEnabledAsync(account, model.Enabled);

            if (result.EmailUnverified)
            {
                await _accountsService.SendTwoFactorCodeEmailAsync(account,
                    Strings.Email_Subject_TwoFactorCode,
                    Strings.Email_Message_TwoFactorCode);

                return BadRequest(new SetTwoFactorEnabledResponseModel
                {
                    ExpectedError = true,
                    EmailUnverified = true
                });
            }

            return Ok();
        }

        /// <summary>
        /// POST: /Account/SendEmailVerificationEmail
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 200 OK and sends email validation email if email is sent successfully.
        /// </returns>
        /// <exception cref="Exception">Thrown if unable to retrieve logged in account</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmailVerificationEmail()
        {
            // nd to check modelstate?

            VakAccount account = await _accountsService.GetApplicationAccountAsync();

            if (account == null)
            {
                // Unexpected error - logged in (got past authorize attribute) but unable to retrieve account
                throw new Exception();
            }

            await _accountsService.SendEmailVerificationEmailAsync(account,
                Strings.Email_Subject_EmailVerification,
                Strings.Email_Message_EmailVerification,
                _urlOptions.ClientDomain);

            return Ok();
        }

        /// <summary>
        /// Post: /Account/SetEmailVerified
        /// </summary>
        /// <returns>
        /// 400 BadRequest and <see cref="SetEmailVerifiedResponseModel"/> if account Id is invalid.
        /// 400 BadRequest and <see cref="SetEmailVerifiedResponseModel"/> if token is invalid or expired.
        /// 200 OK if setting of email verified succeeds.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SetEmailVerified([FromBody] SetEmailVerifiedFormModel model)
        {
            // check model state? incase account id is not a string

            VakAccount account = await _vakAccountRepository.GetAccountAsync(model.AccountId);

            if (account == null)
            {
                return BadRequest(new SetEmailVerifiedResponseModel
                {
                    ExpectedError = true,
                    InvalidAccountId = true
                });
            }

            ValidateTokenResult result = _accountsService.ValidateToken(TokenServiceOptions.DataProtectionTokenService,
                _accountsService.ConfirmEmailTokenPurpose,
                account,
                model.Token);

            if (!result.Valid)
            {
                return BadRequest(new SetEmailVerifiedResponseModel
                {
                    ExpectedError = true,
                    InvalidToken = true
                });
            }

            await _accountsService.SetEmailVerifiedAsync(account, true);

            return Ok();
        }

        /// <summary>
        /// POST: /Account/SendAltEmailVerificationEmail
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 400 BadRequest and <see cref="SendAltEmailVerificationEmailResponseModel"> if account alternative email is invalid. 
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 200 OK and sends alt email validation email if email is sent successfully.
        /// </returns>
        /// <exception cref="Exception">Thrown if unable to retrieve logged in account</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAltEmailVerificationEmail()
        {
            VakAccount account = await _accountsService.GetApplicationAccountAsync();

            if (account == null)
            {
                // Unexpected error - logged in (got past authorize attribute) but unable to retrieve account
                throw new Exception();
            }

            SendAltEmailVerificationEmailResult result = await _accountsService.
                SendAltEmailVerificationEmailAsync(account,
                    Strings.Email_Subject_EmailVerification,
                    Strings.Email_Message_EmailVerification,
                    _urlOptions.ClientDomain);

            if (result.InvalidAltEmail)
            {
                return BadRequest(new SendAltEmailVerificationEmailResponseModel
                {
                    ExpectedError = true,
                    InvalidAltEmail = true
                });
            }

            return Ok();
        }

        /// <summary>
        /// Post: /Account/SetAltEmailVerified
        /// </summary>
        /// <returns>
        /// 400 BadRequest and <see cref="SetAltEmailVerifiedResponseModel"/> if account Id is invalid.
        /// 400 BadRequest and <see cref="SetAltEmailVerifiedResponseModel"/> if token is invalid or expired.
        /// 200 OK if setting of email verified succeeds.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SetAltEmailVerified([FromBody] SetAltEmailVerifiedFormModel model)
        {
            // check model state? incase account id is not a string

            VakAccount account = await _vakAccountRepository.GetAccountAsync(model.AccountId);

            if (account == null)
            {
                return BadRequest(new SetAltEmailVerifiedResponseModel
                {
                    ExpectedError = true,
                    InvalidAccountId = true
                });
            }

            ValidateTokenResult result = _accountsService.ValidateToken(TokenServiceOptions.DataProtectionTokenService,
                _accountsService.ConfirmEmailTokenPurpose,
                account,
                model.Token);

            if (!result.Valid)
            {
                return BadRequest(new SetAltEmailVerifiedResponseModel
                {
                    ExpectedError = true,
                    InvalidToken = true
                });
            }

            await _accountsService.SetAltEmailVerifiedAsync(account, true);

            return Ok();
        }

        /// <summary>
        /// Post: /Account/TwoFactorVerifyEmail
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 400 BadRequest and <see cref="TwoFactorVerifyEmailResponseModel"/> if model state is invalid.
        /// 400 BadRequest and <see cref="TwoFactorVerifyEmailResponseModel"/> if token is expired.
        /// 400 BadRequest and <see cref="TwoFactorVerifyEmailResponseModel"/> if token is invalid.
        /// 200 OK if email verification succeeds.
        /// </returns>
        /// <exception cref="Exception">Thrown if unable to retrieve logged in account</exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TwoFactorVerifyEmail([FromBody] TwoFactorVerifyEmailFormModel model)
        {
            if (ModelState.IsValid)
            {
                VakAccount account = await _accountsService.GetApplicationAccountAsync();

                if (account == null)
                {
                    // Unexpected error - logged in (got past authorize attribute) but unable to retrieve account
                    throw new Exception();
                }

                ValidateTokenResult result = _accountsService.ValidateToken(TokenServiceOptions.TotpTokenService, _accountsService.TwoFactorTokenPurpose, account, model.Code);

                if (result.Valid)
                {
                    await _accountsService.SetEmailVerifiedAsync(account, true);

                    return Ok();
                }

                if(result.Expired)
                {
                    return BadRequest(new TwoFactorVerifyEmailResponseModel
                    {
                        ExpectedError = true,
                        ExpiredToken = true
                    });
                }

                ModelState.AddModelError(nameof(TwoFactorVerifyEmailFormModel.Code), Strings.ErrorMessage_TwoFactorCode_Invalid);
            }

            return BadRequest(new TwoFactorVerifyEmailResponseModel
            {
                ExpectedError = true,
                ModelState = new SerializableError(ModelState)
            });
        }
        #endregion
    }
}
