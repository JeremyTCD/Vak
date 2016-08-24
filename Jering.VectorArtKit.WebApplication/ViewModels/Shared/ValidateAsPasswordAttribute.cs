using Jering.AccountManagement.Security;
using Jering.VectorArtKit.WebApplication.ViewModels.Shared;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Shared
{
    /// <summary>
    /// Validates a password using an <see cref="IPasswordValidationService"/> instance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateAsPasswordAttribute : ValidationAttribute
    {
        /// <summary>
        /// Validates <paramref name="value"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with error message <see cref="ViewModelOptions.Password_Invalid"/> if <paramref name="value"/> cannot be converted into a string.
        /// <see cref="ValidationResult"/> with error message <see cref="ViewModelOptions.Password_TooShort"/> if password is too short.
        /// <see cref="ValidationResult"/> with error message <see cref="ViewModelOptions.Password_NonAlphaNumericRequired"/> if password has no non-alphanumeric characters.
        /// <see cref="ValidationResult"/> with error message <see cref="ViewModelOptions.Password_DigitRequired"/> if password has no digits.
        /// <see cref="ValidationResult"/> with error message <see cref="ViewModelOptions.Password_LowercaseRequired"/> if password has no lowercase characters.
        /// <see cref="ValidationResult"/> with error message <see cref="ViewModelOptions.Password_UppercaseRequired"/> if password has no uppercase characters.
        /// <see cref="ValidationResult.Success"/> if password passes all conditions.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ViewModelOptions viewModelOptions = ((IOptions<ViewModelOptions>)validationContext.GetService(typeof(IOptions<ViewModelOptions>))).Value;

            string valueAsString = value as string;
            if (valueAsString == null)
            {
                return new ValidationResult(viewModelOptions.Password_Invalid);
            }

            IPasswordValidationService passwordValidationService = (IPasswordValidationService)validationContext.GetService(typeof(IPasswordValidationService));
            ValidatePasswordResult validatePasswordResult = passwordValidationService.ValidatePassword(valueAsString);

            switch (validatePasswordResult)
            {
                case (ValidatePasswordResult.TooShort):
                    return new ValidationResult(viewModelOptions.Password_TooShort);

                case (ValidatePasswordResult.NonAlphanumericRequired):
                    return new ValidationResult(viewModelOptions.Password_NonAlphaNumericRequired);

                case (ValidatePasswordResult.DigitRequired):
                    return new ValidationResult(viewModelOptions.Password_DigitRequired);

                case (ValidatePasswordResult.LowercaseRequired):
                    return new ValidationResult(viewModelOptions.Password_LowercaseRequired);

                case (ValidatePasswordResult.UppercaseRequired):
                    return new ValidationResult(viewModelOptions.Password_UppercaseRequired);
            }

            return ValidationResult.Success;
        }
    }
}
