using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates that a properties value is not an in-use email
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateEmailInUseAttribute : ValidationAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public ValidateEmailInUseAttribute(string resourceName, Type resourceType)
        {
            ErrorMessageResourceName = resourceName;
            ErrorMessageResourceType = resourceType;
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }
}
