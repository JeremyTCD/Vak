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
    public class ValidateHasLowercaseAttribute : ValidationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public ValidateHasLowercaseAttribute(string resourceName, Type resourceType) 
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
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> is not a string or has no lowercase characters.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> has at least one lowercase character.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string valueAsString = value as string;
            if (valueAsString == null || !valueAsString.Any(IsLower) )
            {
                return new ValidationResult(ErrorMessageString);
            }

            return ValidationResult.Success;
        }

        private bool IsLower(char c)
        {
            return c >= 'a' && c <= 'z';
        }
    }
}
