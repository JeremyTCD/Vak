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

        /// <summary>
        /// Constructor
        /// </summary>
        public DynamicControlValidatorData(string name, string errorMessage, Dictionary<string, string> options)
        {
            Name = name;
            ErrorMessage = errorMessage;
            Options = options;
        }

        /// <summary>
        /// Creates a <see cref="DynamicControlValidatorData"/> instance from <paramref name="validationAttribute"/>.
        /// </summary>
        /// <param name="validationAttribute"></param>
        public static DynamicControlValidatorData FromValidationAttribute(ValidationAttribute validationAttribute)
        {
            Type validationAttributeType = validationAttribute.GetType();

            string name = char.ToLower(validationAttributeType.Name[0]) + validationAttributeType.Name.Replace("Attribute", "").Substring(1);
            string errorMessage = validationAttribute.ErrorMessageResourceType.GetProperty(validationAttribute.ErrorMessageResourceName).GetValue(null, null) as string;

            PropertyInfo[] propertyInfos = validationAttributeType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            Dictionary<string, string> options = new Dictionary<string, string>();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                // Use ToString here since value may not be a string
                options.Add(propertyInfo.Name, 
                    propertyInfo.GetValue(validationAttribute).ToString());
            }
            return new DynamicControlValidatorData(name, errorMessage, options);
        }
    }
}
