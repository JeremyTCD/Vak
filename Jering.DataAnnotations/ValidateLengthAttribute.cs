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
    public class ValidateLengthAttribute : OptionsValidationAttribute
    {
        private int _length { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        public ValidateLengthAttribute(int length):base(null,null)
        {
            _length = length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <param name="errorMessageProperty"></param>
        /// <param name="optionsType"></param>
        public ValidateLengthAttribute(int length, string errorMessageProperty, Type optionsType) : base(errorMessageProperty, optionsType)
        {
            _length = length;
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
            if (valueAsString == null || valueAsString.Length != _length)
            {
                return new ValidationResult(GetErrorMessage(validationContext));
            }

            return ValidationResult.Success;
        }
    }
}
