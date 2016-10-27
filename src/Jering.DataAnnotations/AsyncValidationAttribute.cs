using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Base class for asynchronous validation attributes.
    /// </summary>
    public abstract class AsyncValidationAttribute : ValidationAttribute
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
        protected AsyncValidationAttribute(string resourceName, Type resourceType, string controller, string action)
        {
            RelativeUrl = $"{controller.Replace("Controller", "")}/{action}";
            ErrorMessageResourceName = resourceName;
            ErrorMessageResourceType = resourceType;
        }
    }
}
