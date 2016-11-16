using System.Collections.Generic;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Data that defines a control validator
    /// </summary>
    public class ValidatorResponseModel
    {
        /// <summary>
        /// Validator name  
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// Validator specific options
        /// </summary>
        public Dictionary<string, string> Options { get; set; }
    }
}
