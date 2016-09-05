using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates an integer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateAllDigitsAttribute : OptionsValidationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMessageProperty"></param>
        /// <param name="optionsType"></param>
        public ValidateAllDigitsAttribute(string errorMessageProperty, Type optionsType) : base(errorMessageProperty, optionsType){}

        /// <summary>
        /// Validates <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> is not a string or 
        /// contains characters are arent digits.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> contains only digits.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string valueAsString = value as string;
            if (valueAsString == null || !valueAsString.All(IsDigit))
            {
                return new ValidationResult(GetErrorMessage(validationContext));
            }

            return ValidationResult.Success;
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }
    }
}
