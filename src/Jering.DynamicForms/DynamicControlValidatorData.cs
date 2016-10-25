using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Holds data that defines a control validator
    /// </summary>
    public class DynamicControlValidatorData
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
