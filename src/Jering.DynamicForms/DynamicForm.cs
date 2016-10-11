using System.Collections.Generic;
using Newtonsoft.Json;

namespace Jering.DynamicForms
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicForm
    {
        /// <summary>
        /// 
        /// </summary>
        public List<DynamicInput> DynamicInputs = new List<DynamicInput>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dynamicInput"></param>
        public void AddDynamicInput(DynamicInput dynamicInput)
        {
            DynamicInputs.Add(dynamicInput);
        } 
    }
}
