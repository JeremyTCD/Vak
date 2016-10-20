using System;
using System.Collections.Generic;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Marks a model property as a dynamic control
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DynamicControlAttribute : Attribute
    {
        /// <summary>
        /// Control Html tag name
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// Position of control relative to other controls in same dynamic form
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Display name for control. Rendered as text content of associated label.
        /// </summary>
        public string DisplayNameResourceName { get; set; }

        /// <summary>
        /// String resource class
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="displayNameResourceName"></param>
        /// <param name="resourceType"></param>
        /// <param name="order"></param>
        public DynamicControlAttribute(
            string tagName,
            string displayNameResourceName,
            Type resourceType,
            int order = 0)
        {
            TagName = tagName;
            Order = order;
            DisplayNameResourceName = displayNameResourceName;
            ResourceType = resourceType;
        }
    }
}
