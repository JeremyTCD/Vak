using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates an email address's format.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateEmailAddressAttribute : ValidationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public ValidateEmailAddressAttribute(string resourceName, Type resourceType)
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
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> isn't an email address.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> is an email address.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> is null or an empty string.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string valueAsString = value as string;
            if (valueAsString == null || valueAsString.Trim().Length == 0)
            {
                return ValidationResult.Success;
            }

            // Email address is valid if there is only 1 '@' character
            // and it is neither the first nor the last character
            bool found = false;
            for (int i = 0; i < valueAsString.Length; i++)
            {
                if (valueAsString[i] == '@')
                {
                    if (found || i == 0 || i == valueAsString.Length - 1)
                    {
                        return new ValidationResult(ErrorMessageString);
                    }
                    found = true;
                }
            }

            return found ? ValidationResult.Success : new ValidationResult(ErrorMessageString);
        }
    }
}
