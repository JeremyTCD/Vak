using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates differing properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateDiffersAttribute : ValidationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string OtherProperty { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherProperty"></param>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public ValidateDiffersAttribute(string otherProperty, string resourceName, Type resourceType)
        {
            ErrorMessageResourceName = resourceName;
            ErrorMessageResourceType = resourceType;
            OtherProperty = otherProperty;
        }

        /// <summary>
        /// Validates <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> does not differ from other property.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> differs from other property .
        /// <see cref="ValidationResult.Success"/> if properties value is null.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            object otherPropertyValue = validationContext.
                                            ObjectType.
                                            GetRuntimeProperty(OtherProperty).
                                            GetValue(validationContext.ObjectInstance, null);

            if (value == null || otherPropertyValue == null || !Equals(value, otherPropertyValue))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessageString);
        }
    }
}
