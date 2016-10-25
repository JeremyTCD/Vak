using System.Collections.Generic;
using System.Reflection;
using System;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Defines a dynamic form. Corresponds to javascript dynamic form type.
    /// </summary>
    public class DynamicFormData
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Dynamic controls
        /// </summary>
        public List<DynamicControlData> DynamicControlDatas = new List<DynamicControlData>();

        /// <summary>
        /// Adds a dynamic control
        /// </summary>
        /// <param name="dynamicControl"></param>
        public void AddDynamicControl(DynamicControlData dynamicControl)
        {
            DynamicControlDatas.Add(dynamicControl);
        } 
    }
}
