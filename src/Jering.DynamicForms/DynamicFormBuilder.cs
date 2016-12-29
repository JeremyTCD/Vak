using Jering.DataAnnotations;
using Jering.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Builds structures used by the dynamic forms system.
    /// </summary>
    public class DynamicFormBuilder : IDynamicFormBuilder
    {
        /// <summary>
        /// Creates a <see cref="DynamicFormData"/> from <paramref name="typeInfo"/>.
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns>
        /// <see cref="DynamicFormData"/> equivalent of <paramref name="typeInfo"/> if <paramref name="typeInfo"/> contains <see cref="DynamicFormAttribute"/>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="typeInfo"/> does not have a <see cref="DynamicFormAttribute"/></exception>
        public virtual DynamicFormData BuildDynamicFormData(TypeInfo typeInfo)
        {
            DynamicFormAttribute dynamicFormAttribute = typeInfo.GetCustomAttribute<DynamicFormAttribute>();
            if (dynamicFormAttribute == null)
            {
                throw new ArgumentException(nameof(typeInfo));
            }

            PropertyInfo[] propertyInfos = typeInfo.GetProperties();
            List<DynamicControlData> dynamicControlData = new List<DynamicControlData>();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                DynamicControlData controlData = BuildDynamicControlData(propertyInfo);
                if (controlData != null)
                    dynamicControlData.Add(controlData);
            }

            string errorMessage = dynamicFormAttribute.
                ResourceType.
                GetProperty(dynamicFormAttribute.ErrorMessageResourceName).
                GetValue(null, null) as string;
            string buttonText = dynamicFormAttribute.
                ResourceType.
                GetProperty(dynamicFormAttribute.ButtonTextResourceName).
                GetValue(null, null) as string;

            return new DynamicFormData() { ButtonText = buttonText, ErrorMessage = errorMessage, DynamicControlData = dynamicControlData };
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
                // Validator responseModel
                IEnumerable<ValidationAttribute> validationAttributes = propertyInfo.GetCustomAttributes<ValidationAttribute>();
                List<ValidatorData> validatorData = new List<ValidatorData>();
                ValidatorData asyncValidatorData = null;
                foreach (ValidationAttribute validationAttribute in validationAttributes)
                {
                    if (validationAttribute is AsyncValidateAttribute)
                    { 
                        asyncValidatorData = BuildDynamicControlValidatorData(validationAttribute);
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
                                                propertyAttribute.
                                                    ResourceType.
                                                    GetTypeInfo().
                                                    GetDeclaredProperty(propertyAttribute.PropertyValueResourceName).
                                                    GetValue(null) as string;
                    properties.Add(propertyAttribute.PropertyName, propertyValue);
                }

                return new DynamicControlData()
                {
                    // Name must start with lowercase character for consistency with request models
                    Name = propertyInfo.Name.firstCharToLowercase(),
                    Order = dynamicControlAttribute.Order,
                    TagName = dynamicControlAttribute.TagName,
                    ValidatorData = validatorData,
                    AsyncValidatorData = asyncValidatorData,
                    DisplayName = dynamicControlAttribute.DisplayNameResourceName != null ? dynamicControlAttribute.
                                    ResourceType.
                                    GetTypeInfo().
                                    GetDeclaredProperty(dynamicControlAttribute.DisplayNameResourceName).
                                    GetValue(null) as string : null,
                    Properties = properties
                };
            }

            return null;
        }

        /// <summary>
        /// Creates a <see cref="ValidatorData"/> instance from <paramref name="validationAttribute"/>.
        /// </summary>
        /// <param name="validationAttribute"></param>
        /// <returns>
        /// <see cref="ValidatorData"/> equivlent of <paramref name="validationAttribute"/>.
        /// </returns>
        public virtual ValidatorData BuildDynamicControlValidatorData(ValidationAttribute validationAttribute)
        {
            Type validationAttributeType = validationAttribute.GetType();

            string name = char.ToLower(validationAttributeType.Name[0]) + validationAttributeType.Name.Replace("Attribute", "").Substring(1);
            string errorMessage = validationAttribute.
                ErrorMessageResourceType.
                GetProperty(validationAttribute.ErrorMessageResourceName).
                GetValue(null, null) as string;

            List<PropertyInfo> propertyInfos = validationAttributeType.
                GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).ToList<PropertyInfo>();

            Dictionary<string, string> options = new Dictionary<string, string>();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                // Use ToString here since value may not be a string
                options.Add(propertyInfo.Name,
                    propertyInfo.GetValue(validationAttribute).ToString());
            }
            return new ValidatorData() {
                Name = name,
                ErrorMessage = errorMessage,
                Options = options
            };
        }
    }
}
