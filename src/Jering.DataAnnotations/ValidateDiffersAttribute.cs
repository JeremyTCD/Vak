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
        /// <see cref="ValidationResult"/> with an error message if other property does not exist.
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> does not differ from other property.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> differs from other property.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            PropertyInfo otherPropertyInfo = validationContext.ObjectType.GetRuntimeProperty(OtherProperty);
            if (otherPropertyInfo == null)
            {
                return new ValidationResult(ErrorMessageString);
            }

            object otherPropertyValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);
            if (Equals(value, otherPropertyValue))
            {
                return new ValidationResult(ErrorMessageString);
            }

            return ValidationResult.Success;
        }
    }
}
