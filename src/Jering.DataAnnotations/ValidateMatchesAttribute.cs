using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates matching properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateMatchesAttribute : ValidationAttribute
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
        public ValidateMatchesAttribute(string otherProperty, string resourceName, Type resourceType)       
        {
            OtherProperty = otherProperty;
            ErrorMessageResourceName = resourceName;
            ErrorMessageResourceType = resourceType;
        }

        /// <summary>
        /// Validates <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> does not match other property.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> matches other property.
        /// <see cref="ValidationResult.Success"/> if either property is null.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            object otherPropertyValue = validationContext.
                                            ObjectType.
                                            GetRuntimeProperty(OtherProperty).
                                            GetValue(validationContext.ObjectInstance, null);


            if (value == null || otherPropertyValue == null || Equals(value, otherPropertyValue))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessageString);
        }
    }
}
