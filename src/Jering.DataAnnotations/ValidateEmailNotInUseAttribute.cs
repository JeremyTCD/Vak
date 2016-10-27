using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates that a properties value is not an in-use email
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class AsyncValidateEmailNotInUseAttribute : AsyncValidationAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public AsyncValidateEmailNotInUseAttribute(string resourceName, Type resourceType, string controller, string action) : base(resourceName, resourceType, controller, action)
        {
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
