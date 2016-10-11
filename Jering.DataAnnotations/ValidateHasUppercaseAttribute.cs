using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates that string has at least one uppercase character.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateHasUppercaseAttribute : ValidationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public ValidateHasUppercaseAttribute(string resourceName, Type resourceType)
        {
            ErrorMessageResourceName = resourceName;
            ErrorMessageResourceType = resourceType;
        }

        /// <summary>
        /// Validates <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> is not a string or has no uppercase characters.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> has at least one uppercase character.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string valueAsString = value as string;
            if (valueAsString == null || !valueAsString.Any(IsUpper) )
            {
                return new ValidationResult(ErrorMessageString);
            }

            return ValidationResult.Success;
        }

        private bool IsUpper(char c)
        {
            return c >= 'A' && c <= 'Z';
        }
    }
}
