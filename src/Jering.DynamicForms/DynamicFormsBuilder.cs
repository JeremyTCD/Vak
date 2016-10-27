using Jering.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Builds structures used by the dynamic forms system.
    /// </summary>
    public class DynamicFormsBuilder : IDynamicFormsBuilder
    {
        /// <summary>
        /// Creates a <see cref="DynamicFormData"/> from <paramref name="dynamicFormAttribute"/>.
        /// </summary>
        /// <param name="dynamicFormAttribute"></param>
        /// <returns>
        /// <see cref="DynamicFormData"/> equivalent of <paramref name="dynamicFormAttribute"/>.
        /// </returns>
        public virtual DynamicFormData BuildDynamicFormData(DynamicFormAttribute dynamicFormAttribute)
        {
            string errorMessage = dynamicFormAttribute.ResourceType.GetProperty(dynamicFormAttribute.ErrorMessageResourceName).GetValue(null, null) as string;

            return new DynamicFormData() { ErrorMessage = errorMessage };
        }


        /// <summary>
        /// Creates a <see cref="DynamicControlData"/> from <paramref name="propertyInfo"/>.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns>
        /// <see cref="DynamicControlData"/> equivalent of <paramref name="propertyInfo"/> if <paramref name="propertyInfo"/> contains <see cref="DynamicControlAttribute"/>.
        /// Otherwise, null.
        /// </returns>
        public virtual DynamicControlData BuildDynamicControlData(PropertyInfo propertyInfo)
        {
            DynamicControlAttribute dynamicControlAttribute = propertyInfo.GetCustomAttribute(typeof(DynamicControlAttribute)) as DynamicControlAttribute;

            if (dynamicControlAttribute != null)
            {
                // Validator data
                IEnumerable<ValidationAttribute> validationAttributes = propertyInfo.GetCustomAttributes<ValidationAttribute>();
                List<DynamicControlValidatorData> validatorData = new List<DynamicControlValidatorData>();
                DynamicControlValidatorData asyncValidatorData = null;
                foreach (ValidationAttribute validationAttribute in validationAttributes)
                {
                    if (validationAttribute is AsyncValidationAttribute)
                    { 
                        if(asyncValidatorData == null)
                        {
                            asyncValidatorData = BuildDynamicControlValidatorData(validationAttribute);
                        }else
                        {
                            throw new ArgumentException($"{nameof(propertyInfo)} cannot have more than 1 {nameof(AsyncValidationAttribute)}");
                        }
                    }
                    else
                    {
                        validatorData.Add(BuildDynamicControlValidatorData(validationAttribute));
                    }
                }

                // Html properties
                IEnumerable<DynamicControlPropertyAttribute> propertyAttributes = propertyInfo.GetCustomAttributes<DynamicControlPropertyAttribute>();
                Dictionary<string, string> properties = new Dictionary<string, string>();
                foreach (DynamicControlPropertyAttribute propertyAttribute in propertyAttributes)
                {
                    string propertyValue = propertyAttribute.PropertyValue != null ?
                                                propertyAttribute.PropertyValue :
                                                propertyAttribute.ResourceType.GetTypeInfo().GetDeclaredProperty(propertyAttribute.PropertyValueResourceName).GetValue(null) as string;
                    properties.Add(propertyAttribute.PropertyName, propertyValue);
                }

                return new DynamicControlData()
                {
                    Name = propertyInfo.Name,
                    Order = dynamicControlAttribute.Order,
                    TagName = dynamicControlAttribute.TagName,
                    ValidatorData = validatorData,
                    AsyncValidatorData = asyncValidatorData,
                    DisplayName = dynamicControlAttribute.
                                    ResourceType.
                                    GetTypeInfo().
                                    GetDeclaredProperty(dynamicControlAttribute.DisplayNameResourceName).
                                    GetValue(null) as string,
                    Properties = properties
                };
            }

            return null;
        }

        /// <summary>
        /// Creates a <see cref="DynamicControlValidatorData"/> instance from <paramref name="validationAttribute"/>.
        /// </summary>
        /// <param name="validationAttribute"></param>
        /// <returns>
        /// <see cref="DynamicControlValidatorData"/> equivlent of <paramref name="validationAttribute"/>.
        /// </returns>
        public virtual DynamicControlValidatorData BuildDynamicControlValidatorData(ValidationAttribute validationAttribute)
        {
            Type validationAttributeType = validationAttribute.GetType();

            string name = char.ToLower(validationAttributeType.Name[0]) + validationAttributeType.Name.Replace("Attribute", "").Substring(1);
            string errorMessage = validationAttribute.ErrorMessageResourceType.GetProperty(validationAttribute.ErrorMessageResourceName).GetValue(null, null) as string;

            List<PropertyInfo> propertyInfos = validationAttributeType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).ToList<PropertyInfo>();
            if(validationAttribute is AsyncValidationAttribute)
            {
                propertyInfos.Add(validationAttributeType.GetProperty(nameof(AsyncValidationAttribute.RelativeUrl)));
            }

            Dictionary<string, string> options = new Dictionary<string, string>();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                // Use ToString here since value may not be a string
                options.Add(propertyInfo.Name,
                    propertyInfo.GetValue(validationAttribute).ToString());
            }
            return new DynamicControlValidatorData() {
                Name = name,
                ErrorMessage = errorMessage,
                Options = options
            };
        }
    }
}
