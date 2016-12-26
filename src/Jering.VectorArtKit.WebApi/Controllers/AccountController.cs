using Jering.Accounts.DatabaseInterface;
using Jering.Utilities;
using Jering.VectorArtKit.WebApi.RequestModels;
using Jering.VectorArtKit.WebApi.Resources;
using Jering.VectorArtKit.WebApi.ResponseModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Jering.Accounts;
using Microsoft.AspNetCore.Antiforgery;
using Jering.VectorArtKit.WebApi.Extensions;
using Jering.VectorArtKit.DatabaseInterface;

namespace Jering.VectorArtKit.WebApi.Controllers
{
    [Authorize]
    [ValidateAntiForgeryToken]
    public class AccountController : Controller
    {
        private IAccountService<VakAccount> _accountService;
        private IAccountRepository<VakAccount> _vakAccountRepository;
        private IAntiforgery _antiforgery;

        public AccountController(
            IAccountRepository<VakAccount> vakAccountRepository,
            IAccountService<VakAccount> accountService,
            IAntiforgery antiforgery
            )
        {
            _vakAccountRepository = vakAccountRepository;
            _accountService = accountService;
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
        public async Task<IActionResult> LogIn([FromBody] LogInRequestModel model)
        {
            if (ModelState.IsValid)
            {
                LogInActionResult result = await _accountService.LogInActionAsync(model.Email, model.Password, model.RememberMe);

                if (result == LogInActionResult.TwoFactorRequired)
                {
                    return BadRequest(new LogInResponseModel
                    {
                        ExpectedError = true,
                        TwoFactorRequired = true
                    });
                }

                if (result == LogInActionResult.Success)
                {
                    _antiforgery.AddAntiforgeryCookies(HttpContext);

                    return Ok();
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
        /// 200 OK and application cookie (with empty string values) if auth succeeds. 
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
            await _accountService.ApplicationLogOffAsync();
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
        /// 400 BadRequest and <see cref="TwoFactorLogInResponseModel"/> if code is expired or invalid. 
        /// 200 OK, <see cref="TwoFactorLogInResponseModel"/>, two factor cookie, application 
        /// cookie and antiforgery cookies if login succeeds.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> TwoFactorLogIn([FromBody] TwoFactorLogInRequestModel model)
        {
            if (ModelState.IsValid)
            {
                TwoFactorLogInActionResult result = await _accountService.TwoFactorLogInActionAsync(model.Code, model.IsPersistent);

                if (result == TwoFactorLogInActionResult.InvalidCredentials)
                {
                    return BadRequest(new TwoFactorLogInResponseModel
                    {
                        ExpectedError = true,
                        ExpiredCredentials = true
                    });
                }

                if (result == TwoFactorLogInActionResult.Success)
                {
                    _antiforgery.AddAntiforgeryCookies(HttpContext);

                    return Ok();
                }

                ModelState.AddModelError(nameof(TwoFactorLogInRequestModel.Code), Strings.ErrorMessage_TwoFactorCode_InvalidOrExpired);
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
        /// 200 OK, <see cref="SignUpResponseModel"/>, application cookie, antiforgery cookies if account is 
        /// created successfully.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequestModel model)
        {
            if (ModelState.IsValid)
            {
                SignUpActionResult result = await _accountService.SignUpActionAsync(model.Email, model.Password);

                if (result == SignUpActionResult.Success)
                {
                    _antiforgery.AddAntiforgeryCookies(HttpContext);

                    return Ok();
                }

                ModelState.AddModelError(nameof(SignUpRequestModel.Email), Strings.ErrorMessage_Email_InUse);
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
        public async Task<IActionResult> SendResetPasswordEmail([FromBody] SendResetPasswordEmailRequestModel model)
        {
            if (ModelState.IsValid)
            {
                SendResetPasswordEmailActionResult result = await _accountService.SendResetPasswordEmailActionAsync(model.Email);

                if (result == SendResetPasswordEmailActionResult.Success)
                {
                    return Ok();
                }

                ModelState.AddModelError(nameof(SendResetPasswordEmailRequestModel.Email), Strings.ErrorMessage_Email_Invalid);
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
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel model)
        {
            if (ModelState.IsValid)
            {
                ResetPasswordActionResult result = await _accountService.ResetPasswordActionAsync(model.Email, model.Token, model.NewPassword);

                if (result == ResetPasswordActionResult.InvalidEmail)
                {
                    return BadRequest(new ResetPasswordResponseModel
                    {
                        ExpectedError = true,
                        InvalidEmail = true
                    });
                }

                if (result == ResetPasswordActionResult.InvalidToken)
                {
                    return BadRequest(new ResetPasswordResponseModel
                    {
                        ExpectedError = true,
                        InvalidToken = true
                    });
                }

                if (result == ResetPasswordActionResult.Success)
                {
                    return Ok();
                }

                ModelState.AddModelError(nameof(SetPasswordRequestModel.NewPassword), Strings.ErrorMessage_NewPassword_MustDiffer);
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
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 200 OK and <see cref="GetAccountDetailsResponseModel"/> if auth succeeds.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAccountDetails()
        {
            VakAccount account = await _accountService.GetAccountDetailsActionAsync();

            if (account == null)
            {
                // Logged in but unable to retrieve account
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
        /// 400 BadRequest and <see cref="SetPasswordResponseModel"/> if new password is same as current password. 
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 200 OK and application cookie if password change succeeds.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequestModel model)
        {
            if (ModelState.IsValid)
            {
                SetPasswordActionResult result = await _accountService.SetPasswordActionAsync(model.CurrentPassword, model.NewPassword);

                if (result == SetPasswordActionResult.NoLoggedInAccount)
                {
                    // Logged in but unable to retrieve account 
                    throw new Exception();
                }

                if (result == SetPasswordActionResult.Success)
                {
                    return Ok();
                }

                if (result == SetPasswordActionResult.AlreadySet)
                {
                    ModelState.AddModelError(nameof(SetPasswordRequestModel.NewPassword), Strings.ErrorMessage_NewPassword_MustDiffer);
                }
                else if (result == SetPasswordActionResult.InvalidCurrentPassword)
                {
                    ModelState.AddModelError(nameof(SetPasswordRequestModel.CurrentPassword), Strings.ErrorMessage_Password_Invalid);
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
        [HttpPost]
        public async Task<IActionResult> SetEmail([FromBody] SetEmailRequestModel model)
        {
            if (ModelState.IsValid)
            {
                SetEmailActionResult result = await _accountService.SetEmailActionAsync(model.Password, model.NewEmail);

                if (result == SetEmailActionResult.NoLoggedInAccount)
                {
                    // Logged in but unable to retrieve account 
                    throw new Exception();
                }

                if (result == SetEmailActionResult.Success)
                {
                    return Ok();
                }

                if (result == SetEmailActionResult.AlreadySet)
                {
                    ModelState.AddModelError(nameof(SetEmailRequestModel.NewEmail), Strings.ErrorMessage_NewEmail_MustDiffer);
                }
                else if (result == SetEmailActionResult.EmailInUse)
                {
                    ModelState.AddModelError(nameof(SetEmailRequestModel.NewEmail), Strings.ErrorMessage_Email_InUse);
                }
                else
                {
                    ModelState.AddModelError(nameof(SetEmailRequestModel.Password), Strings.ErrorMessage_Password_Invalid);
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
        /// 400 BadRequest and <see cref="SetAltEmailResponseModel"/> if new alternative email is the same 
        /// as current alternative email. 
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 200 OK if alternative email change succeeds.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> SetAltEmail([FromBody] SetAltEmailRequestModel model)
        {
            if (ModelState.IsValid)
            {
                SetAltEmailActionResult result = await _accountService.SetAltEmailActionAsync(model.Password, model.NewAltEmail);

                if (result == SetAltEmailActionResult.NoLoggedInAccount)
                {
                    // Logged in but unable to retrieve account 
                    throw new Exception();
                }

                if (result == SetAltEmailActionResult.Success)
                {
                    return Ok();
                }

                if (result == SetAltEmailActionResult.AlreadySet)
                {
                    ModelState.AddModelError(nameof(SetAltEmailRequestModel.NewAltEmail), Strings.ErrorMessage_NewEmail_MustDiffer);
                }
                else
                {
                    ModelState.AddModelError(nameof(SetAltEmailRequestModel.Password), Strings.ErrorMessage_Password_Invalid);
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
        [HttpPost]
        public async Task<IActionResult> SetDisplayName([FromBody] SetDisplayNameRequestModel model)
        {
            if (ModelState.IsValid)
            {
                SetDisplayNameActionResult result = await _accountService.SetDisplayNameActionAsync(model.Password, model.NewDisplayName);

                if (result == SetDisplayNameActionResult.NoLoggedInAccount)
                {
                    // Logged in but unable to retrieve account 
                    throw new Exception();
                }

                if (result == SetDisplayNameActionResult.Success)
                {
                    return Ok();
                }

                if (result == SetDisplayNameActionResult.AlreadySet)
                {
                    ModelState.AddModelError(nameof(SetDisplayNameRequestModel.NewDisplayName), Strings.ErrorMessage_NewDisplayName_MustDiffer);
                }
                else if (result == SetDisplayNameActionResult.DisplayNameInUse)
                {
                    ModelState.AddModelError(nameof(SetDisplayNameRequestModel.NewDisplayName), Strings.ErrorMessage_DisplayName_InUse);
                }
                else
                {
                    ModelState.AddModelError(nameof(SetDisplayNameRequestModel.Password), Strings.ErrorMessage_Password_Invalid);
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
        [HttpPost]
        public async Task<IActionResult> SetTwoFactorEnabled([FromBody] SetTwoFactorEnabledRequestModel model)
        {
            SetTwoFactorEnabledActionResult result = await _accountService.SetTwoFactorEnabledActionAsync(model.Enabled);

            if (result == SetTwoFactorEnabledActionResult.NoLoggedInAccount)
            {
                // Logged in but unable to retrieve account 
                throw new Exception();
            }

            if (result == SetTwoFactorEnabledActionResult.EmailUnverified)
            {
                return BadRequest(new SetTwoFactorEnabledResponseModel
                {
                    ExpectedError = true,
                    EmailUnverified = true
                });
            }

            return Ok();
        }

        /// <summary>
        /// Post: /Account/SetEmailVerified
        /// </summary>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 400 BadRequest and <see cref="SetEmailVerifiedResponseModel"/> if token is invalid or expired.
        /// 200 OK if setting of email verified succeeds.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> SetEmailVerified([FromBody] SetEmailVerifiedRequestModel model)
        {
            SetEmailVerifiedActionResult result = await _accountService.SetEmailVerifiedActionAsync(model.Token);

            if (result == SetEmailVerifiedActionResult.NoLoggedInAccount)
            {
                // Logged in but unable to retrieve account 
                throw new Exception();
            }

            if (result == SetEmailVerifiedActionResult.InvalidToken)
            {
                return BadRequest(new SetEmailVerifiedResponseModel
                {
                    ExpectedError = true,
                    InvalidToken = true
                });
            }

            return Ok();
        }

        /// <summary>
        /// Post: /Account/SetAltEmailVerified
        /// </summary>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 400 BadRequest and <see cref="SetAltEmailVerifiedResponseModel"/> if token is invalid or expired.
        /// 200 OK if setting of email verified succeeds.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> SetAltEmailVerified([FromBody] SetAltEmailVerifiedRequestModel model)
        {
            // check model state? incase account id is not a string

            SetAltEmailVerifiedActionResult result = await _accountService.SetAltEmailVerifiedActionAsync(model.Token);

            if (result == SetAltEmailVerifiedActionResult.NoLoggedInAccount)
            {
                // Logged in but unable to retrieve account 
                throw new Exception();
            }

            if (result == SetAltEmailVerifiedActionResult.InvalidToken)
            {
                return BadRequest(new SetAltEmailVerifiedResponseModel
                {
                    ExpectedError = true,
                    InvalidToken = true
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
        [HttpPost]
        public async Task<IActionResult> SendEmailVerificationEmail()
        {
            SendEmailVerificationEmailActionResult result = await _accountService.SendEmailVerificationEmailActionAsync();

            if (result == SendEmailVerificationEmailActionResult.NoLoggedInAccount)
            {
                // Logged in but unable to retrieve account 
                throw new Exception();
            }

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
        public async Task<IActionResult> SendAltEmailVerificationEmail()
        {
            SendAltEmailVerificationEmailActionResult result = await _accountService.SendAltEmailVerificationEmailActionAsync();

            if (result == SendAltEmailVerificationEmailActionResult.NoLoggedInAccount)
            {
                // Logged in but unable to retrieve account 
                throw new Exception();
            }

            if (result == SendAltEmailVerificationEmailActionResult.NoAltEmail)
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
        /// Post: /Account/TwoFactorVerifyEmail
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// 400 BadRequest and <see cref="ErrorResponseModel"/> if anti-forgery credentials are invalid.
        /// 401 Unauthorized and <see cref="ErrorResponseModel>"/> if auth fails.
        /// 400 BadRequest and <see cref="TwoFactorVerifyEmailResponseModel"/> if model state is invalid.
        /// 400 BadRequest and <see cref="TwoFactorVerifyEmailResponseModel"/> if token is expired or invalid.
        /// 200 OK if email verification succeeds.
        /// </returns>
        /// <exception cref="Exception">Thrown if unable to retrieve logged in account</exception>
        [HttpPost]
        public async Task<IActionResult> TwoFactorVerifyEmail([FromBody] TwoFactorVerifyEmailRequestModel model)
        {
            if (ModelState.IsValid)
            {
                TwoFactorVerifyEmailActionResult result = await _accountService.TwoFactorVerifyEmailActionAsync(model.Code);

                if (result == TwoFactorVerifyEmailActionResult.NoLoggedInAccount)
                {
                    // Logged in but unable to retrieve account 
                    throw new Exception();
                }

                if (result == TwoFactorVerifyEmailActionResult.InvalidCode)
                {
                    ModelState.AddModelError(nameof(TwoFactorVerifyEmailRequestModel.Code), Strings.ErrorMessage_TwoFactorCode_InvalidOrExpired);
                }
                else
                {
                    return Ok();
                }
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
