using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Asynchronous validation attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AsyncValidateAttribute : ValidationAttribute
    {
        /// <summary>
        /// Web Api endpoint
        /// </summary>
        public string RelativeUrl { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public AsyncValidateAttribute(string resourceName, Type resourceType, string controller, string action)
        {
            RelativeUrl = $"{controller.Replace("Controller", "")}/{action}";
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
