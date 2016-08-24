using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Provides password validation service.
    /// </summary>
    public class PasswordValidationService : IPasswordValidationService
    {
        private PasswordOptions _passwordOptions;

        /// <summary>
        /// Constructs an instance of <see cref="PasswordValidationService"/> .
        /// </summary>
        /// <param name="passwordOptionsAccessor"></param>
        public PasswordValidationService(IOptions<PasswordOptions> passwordOptionsAccessor)
        {
            _passwordOptions = passwordOptionsAccessor.Value;
        }

        /// <summary>
        /// Validates <paramref name="password"/> according to options specified by <see cref="PasswordOptions"/> . 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public ValidatePasswordResult ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < _passwordOptions.RequiredLength)
            {
                return ValidatePasswordResult.TooShort;
            }
            if (_passwordOptions.NonAlphanumericRequired && password.All(IsLetterOrDigit))
            {
                return ValidatePasswordResult.NonAlphanumericRequired;
            }
            if (_passwordOptions.DigitRequired && !password.Any(IsDigit))
            {
                return ValidatePasswordResult.DigitRequired;
            }
            if (_passwordOptions.LowerCaseRequired && !password.Any(IsLower))
            {
                return ValidatePasswordResult.LowercaseRequired;
            }
            if (_passwordOptions.UpperCaseRequired && !password.Any(IsUpper))
            {
                return ValidatePasswordResult.UppercaseRequired;
            }
            return ValidatePasswordResult.Valid;
        }

        /// <summary>
        /// Returns a flag indicting whether the supplied character is a digit.
        /// </summary>
        /// <param name="c">The character to check if it is a digit.</param>
        /// <returns>True if the character is a digit, otherwise false.</returns>
        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        /// <summary>
        /// Returns a flag indicting whether the supplied character is a lower case ASCII letter.
        /// </summary>
        /// <param name="c">The character to check if it is a lower case ASCII letter.</param>
        /// <returns>True if the character is a lower case ASCII letter, otherwise false.</returns>
        private bool IsLower(char c)
        {
            return c >= 'a' && c <= 'z';
        }

        /// <summary>
        /// Returns a flag indicting whether the supplied character is an upper case ASCII letter.
        /// </summary>
        /// <param name="c">The character to check if it is an upper case ASCII letter.</param>
        /// <returns>True if the character is an upper case ASCII letter, otherwise false.</returns>
        private bool IsUpper(char c)
        {
            return c >= 'A' && c <= 'Z';
        }

        /// <summary>
        /// Returns a flag indicting whether the supplied character is an ASCII letter or digit.
        /// </summary>
        /// <param name="c">The character to check if it is an ASCII letter or digit.</param>
        /// <returns>True if the character is an ASCII letter or digit, otherwise false.</returns>
        private bool IsLetterOrDigit(char c)
        {
            return IsUpper(c) || IsLower(c) || IsDigit(c);
        }
    }
}
