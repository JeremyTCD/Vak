﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates an integer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateAllDigitsAttribute : ValidationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public ValidateAllDigitsAttribute(string resourceName, Type resourceType){
            ErrorMessageResourceName = resourceName;
            ErrorMessageResourceType = resourceType;
        }

        /// <summary>
        /// Validates <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> is not a string or 
        /// contains characters are arent digits.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> contains only digits.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> is null or an empty string.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string valueAsString = value as string;
            if (valueAsString == null || valueAsString.Trim().Length == 0 || !valueAsString.Any(IsNotDigit))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessageString);
        }

        private bool IsNotDigit(char c)
        {
            return c < '0' || c > '9';
        }
    }
}
