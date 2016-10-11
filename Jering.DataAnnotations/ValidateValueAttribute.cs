using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates a properties value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateValueAttribute : ValidationAttribute
    {
        private object ExpectedValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expectedValue"></param>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public ValidateValueAttribute(object expectedValue, string resourceName, Type resourceType)
        {
            ExpectedValue = expectedValue;
            ErrorMessageResourceName = resourceName;
            ErrorMessageResourceType = resourceType;
        }
        /// <summary>
        /// Validates <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> does not match expected value.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> matches expected value.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!Equals(value, ExpectedValue))
            {
                return new ValidationResult(ErrorMessageString);
            }

            return ValidationResult.Success;
        }
    }
}
