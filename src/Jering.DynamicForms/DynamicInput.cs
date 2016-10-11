using System.Collections.Generic;

namespace Jering.DynamicForms
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicInput
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string InitialValue { get; set; }
        /// <summary>
        /// email, password etc
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// input, select etc
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<DynamicInputValidatorData> ValidatorData { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Placeholder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool RenderLabel { get; set; }
    }
}
