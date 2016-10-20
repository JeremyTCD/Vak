using System.Collections.Generic;
using Newtonsoft.Json;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Defines a dynamic form. Corresponds to javascript dynamic form type.
    /// </summary>
    public class DynamicFormData
    {
        /// <summary>
        /// Dynamic controls
        /// </summary>
        public List<DynamicControlData> DynamicControlDatas = new List<DynamicControlData>();

        /// <summary>
        /// Adds a dynamic control to the dynamic form
        /// </summary>
        /// <param name="dynamicControl"></param>
        public void AddDynamicControl(DynamicControlData dynamicControl)
        {
            DynamicControlDatas.Add(dynamicControl);
        } 
    }
}
