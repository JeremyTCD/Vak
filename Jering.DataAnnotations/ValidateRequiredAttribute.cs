using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates that a required property has a value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateRequiredAttribute : ValidationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public ValidateRequiredAttribute(string resourceName, Type resourceType)
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
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> is null or an empty string.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> is not null or an empty string.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult(ErrorMessageString);
            }

            string valueAsString = value as string;
            if(valueAsString != null && valueAsString.Trim().Length == 0)
            {
                return new ValidationResult(ErrorMessageString);
            }

            return ValidationResult.Success;
        }
    }
}
