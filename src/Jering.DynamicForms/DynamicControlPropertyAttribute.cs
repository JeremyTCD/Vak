using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Defines a Html property to be added to the relevant control
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DynamicControlPropertyAttribute : Attribute
    {
        /// <summary>
        /// Html property name
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Html property value
        /// </summary>
        public string PropertyValue { get; set; }

        /// <summary>
        /// Alternative to <see cref="PropertyValue"/>
        /// </summary>
        public string PropertyValueResourceName { get; set; }

        /// <summary>
        /// String resource class
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <param name="propertyValueResourceName"></param>
        /// <param name="resourceType"></param>
        public DynamicControlPropertyAttribute(
            string propertyName,
            string propertyValue = null,
            string propertyValueResourceName = null,
            Type resourceType = null)
        {
            PropertyName = propertyName;
            PropertyValue = propertyValue;
            PropertyValueResourceName = propertyValueResourceName;
            ResourceType = resourceType;
        }
    }
}
