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
    public class ValidateMinLengthAttribute : OptionsValidationAttribute
    {
        private int _minLength { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minLength"></param>
        /// <param name="errorMessageProperty"></param>
        /// <param name="optionsType"></param>
        public ValidateMinLengthAttribute(int minLength, string errorMessageProperty, Type optionsType) : base(errorMessageProperty, optionsType)
        {
            _minLength = minLength;
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
            if (valueAsString == null || valueAsString.Length < _minLength)
            {
                return new ValidationResult(GetErrorMessage(validationContext));
            }

            return ValidationResult.Success;
        }
    }
}
