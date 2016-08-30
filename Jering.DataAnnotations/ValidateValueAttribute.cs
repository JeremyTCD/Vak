using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates a properties value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateValueAttribute : OptionsValidationAttribute
    {
        private object _expectedValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expectedValue"></param>
        /// <param name="errorMessageProperty"></param>
        /// <param name="optionsType"></param>
        public ValidateValueAttribute(object expectedValue, string errorMessageProperty, Type optionsType):base(errorMessageProperty, optionsType)
        {
            _expectedValue = expectedValue;
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
            if (!Equals(value, _expectedValue))
            {
                return new ValidationResult(GetErrorMessage(validationContext));
            }

            return ValidationResult.Success;
        }
    }
}
