using System;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Marks a model as a dynamic form
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DynamicFormAttribute : Attribute
    {
        /// <summary>
        /// Error message for form. Rendered when user attempts to submit an invalid form.
        /// </summary>
        public string ErrorMessageResourceName { get; set; }

        /// <summary>
        /// String resource class
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errorMessageResourceName"></param>
        /// <param name="resourceType"></param>
        public DynamicFormAttribute(
            string errorMessageResourceName,
            Type resourceType)
        {
            ErrorMessageResourceName = errorMessageResourceName;
            ResourceType = resourceType;
        }
    }
}
