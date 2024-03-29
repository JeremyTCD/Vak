﻿using System.Collections.Generic;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Data that defines a dynamic control.
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
        /// List of <see cref="ValidatorData"/>
        /// </summary>
        public List<ValidatorData> ValidatorData { get; set; }

        /// <summary>
        /// Async validator <see cref="ValidatorData"/> 
        /// </summary>
        public ValidatorData AsyncValidatorData { get; set; }
    }
}
