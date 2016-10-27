using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Defines a dynamic control. Corresponds to javascript dynamic control type.
    /// </summary>
    public class DynamicControlData
    {
        /// <summary>
        /// Control name
        /// </summary>
        public string Name { get; set; }

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
        public string DisplayName { get; set; }

        /// <summary>
        /// Html element properties
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// Control validators data
        /// </summary>
        public List<DynamicControlValidatorData> ValidatorData { get; set; }

        /// <summary>
        /// Control async validators data
        /// </summary>
        public DynamicControlValidatorData AsyncValidatorData { get; set; }
    }
}
