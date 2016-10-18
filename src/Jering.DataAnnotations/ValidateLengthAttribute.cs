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
    public class ValidateLengthAttribute : ValidationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public ValidateLengthAttribute(int length, string resourceName, Type resourceType)
        {
            Length = length;
            ErrorMessageResourceName = resourceName;
            ErrorMessageResourceType = resourceType;
        }

        /// <summary>
        /// Validates <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> is not the expected length.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> is the right length.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> is null or an empty string.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string valueAsString = value as string;
            if (valueAsString == null || valueAsString.Trim().Length == 0 || valueAsString.Length == Length)
            {
                return ValidationResult.Success;
            }
                  
            return new ValidationResult(ErrorMessageString);
        }
    }
}
