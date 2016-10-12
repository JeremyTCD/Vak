using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates property length.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateMinLengthAttribute : ValidationAttribute
    {
        private int MinLength { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minLength"></param>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public ValidateMinLengthAttribute(int minLength, string resourceName, Type resourceType)
        {
            MinLength = minLength;
            ErrorMessageResourceName = resourceName;
            ErrorMessageResourceType = resourceType;
        }

        /// <summary>
        /// Validates <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> is not a string.
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> is too short.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> is long enough.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string valueAsString = value as string;
            if (valueAsString == null || valueAsString.Length < MinLength)
            {
                return new ValidationResult(ErrorMessageString);
            }

            return ValidationResult.Success;
        }
    }
}
