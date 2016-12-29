using System.Collections.Generic;
using System.Reflection;
using System;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Data that defines a dynamic form
    /// </summary>
    public class DynamicFormData
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Button text
        /// </summary>
        public string ButtonText { get; set; }

        /// <summary>
        /// Dynamic controls
        /// </summary>
        public List<DynamicControlData> DynamicControlData { get; set; }
    }
}
