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
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> is not a string.
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> is not the expected length.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> is the right length.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string valueAsString = value as string;
            if (valueAsString == null || valueAsString.Length != Length)
            {
                return new ValidationResult(ErrorMessageString);
            }

            return ValidationResult.Success;
        }
    }
}
