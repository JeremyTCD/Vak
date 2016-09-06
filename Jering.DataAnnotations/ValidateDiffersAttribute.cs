using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates differing properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateDiffersAttribute : OptionsValidationAttribute
    {
        private string _otherProperty { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherProperty"></param>
        public ValidateDiffersAttribute(string otherProperty):this(otherProperty, null, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherProperty"></param>
        /// <param name="errorMessageProperty"></param>
        /// <param name="optionsType"></param>
        public ValidateDiffersAttribute(string otherProperty, string errorMessageProperty, Type optionsType):base(errorMessageProperty, optionsType)
        {
            _otherProperty = otherProperty;
        }

        /// <summary>
        /// Validates <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with an error message if other property does not exist.
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> does not differ from other property.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> differs from other property.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            PropertyInfo otherPropertyInfo = validationContext.ObjectType.GetRuntimeProperty(_otherProperty);
            if (otherPropertyInfo == null)
            {
                return new ValidationResult(GetErrorMessage(validationContext));
            }

            object otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);
            if (Equals(value, otherPropertyValue))
            {
                return new ValidationResult(GetErrorMessage(validationContext));
            }

            return ValidationResult.Success;
        }
    }
}
