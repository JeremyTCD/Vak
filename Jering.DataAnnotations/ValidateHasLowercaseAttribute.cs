using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates that string has at least one lowercase character.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateHasLowercaseAttribute : OptionsValidationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public ValidateHasLowercaseAttribute() : base(null, null) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMessageProperty"></param>
        /// <param name="optionsType"></param>
        public ValidateHasLowercaseAttribute(string errorMessageProperty, Type optionsType) : base(errorMessageProperty, optionsType)
        {
        }

        /// <summary>
        /// Validates <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> is not a string or has no lowercase characters.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> has at least one lowercase character.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string valueAsString = value as string;
            if (valueAsString == null || !valueAsString.Any(IsLower) )
            {
                return new ValidationResult(GetErrorMessage(validationContext));
            }

            return ValidationResult.Success;
        }

        private bool IsLower(char c)
        {
            return c >= 'a' && c <= 'z';
        }
    }
}
