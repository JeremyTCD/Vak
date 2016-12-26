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
    public class DynamicFormsBuilder : IDynamicFormsBuilder
    {
        /// <summary>
        /// Creates a <see cref="DynamicFormResponseModel"/> from <paramref name="typeInfo"/>.
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns>
        /// <see cref="DynamicFormResponseModel"/> equivalent of <paramref name="typeInfo"/> if <paramref name="typeInfo"/> contains <see cref="DynamicFormAttribute"/>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="typeInfo"/> does not have a <see cref="DynamicFormAttribute"/></exception>
        public virtual DynamicFormResponseModel BuildDynamicFormResponseModel(TypeInfo typeInfo)
        {
            DynamicFormAttribute dynamicFormAttribute = typeInfo.GetCustomAttribute<DynamicFormAttribute>();
            if (dynamicFormAttribute == null)
            {
                throw new ArgumentException(nameof(typeInfo));
            }

            PropertyInfo[] propertyInfos = typeInfo.GetProperties();
            List<DynamicControlResponseModel> dynamicControlResponseModels = new List<DynamicControlResponseModel>();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                DynamicControlResponseModel controlResponseModel = BuildDynamicControlResponseModel(propertyInfo);
                if (controlResponseModel != null)
                    dynamicControlResponseModels.Add(controlResponseModel);
            }

            string errorMessage = dynamicFormAttribute.ResourceType.GetProperty(dynamicFormAttribute.ErrorMessageResourceName).GetValue(null, null) as string;
            string buttonText = dynamicFormAttribute.ResourceType.GetProperty(dynamicFormAttribute.ButtonTextResourceName).GetValue(null, null) as string;

            return new DynamicFormResponseModel() { ButtonText = buttonText, ErrorMessage = errorMessage, DynamicControlResponseModels = dynamicControlResponseModels };
        }


        /// <summary>
        /// Creates a <see cref="DynamicControlResponseModel"/> from <paramref name="propertyInfo"/>.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns>
        /// <see cref="DynamicControlResponseModel"/> equivalent of <paramref name="propertyInfo"/> if <paramref name="propertyInfo"/> contains <see cref="DynamicControlAttribute"/>.
        /// Otherwise, null.
        /// </returns>
        public virtual DynamicControlResponseModel BuildDynamicControlResponseModel(PropertyInfo propertyInfo)
        {
            DynamicControlAttribute dynamicControlAttribute = propertyInfo.GetCustomAttribute(typeof(DynamicControlAttribute)) as DynamicControlAttribute;

            if (dynamicControlAttribute != null)
            {
                // Validator responseModel
                IEnumerable<ValidationAttribute> validationAttributes = propertyInfo.GetCustomAttributes<ValidationAttribute>();
                List<ValidatorResponseModel> validatorResponseModel = new List<ValidatorResponseModel>();
                ValidatorResponseModel asyncValidatorResponseModel = null;
                foreach (ValidationAttribute validationAttribute in validationAttributes)
                {
                    if (validationAttribute is AsyncValidateAttribute)
                    { 
                        asyncValidatorResponseModel = BuildDynamicControlValidatorResponseModel(validationAttribute);
                    }
                    else
                    {
                        validatorResponseModel.Add(BuildDynamicControlValidatorResponseModel(validationAttribute));
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

                return new DynamicControlResponseModel()
                {
                    // Name must start with lowercase character for consistency with request models
                    Name = propertyInfo.Name.firstCharToLowercase(),
                    Order = dynamicControlAttribute.Order,
                    TagName = dynamicControlAttribute.TagName,
                    ValidatorResponseModels = validatorResponseModel,
                    AsyncValidatorResponseModel = asyncValidatorResponseModel,
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
        /// Creates a <see cref="ValidatorResponseModel"/> instance from <paramref name="validationAttribute"/>.
        /// </summary>
        /// <param name="validationAttribute"></param>
        /// <returns>
        /// <see cref="ValidatorResponseModel"/> equivlent of <paramref name="validationAttribute"/>.
        /// </returns>
        public virtual ValidatorResponseModel BuildDynamicControlValidatorResponseModel(ValidationAttribute validationAttribute)
        {
            Type validationAttributeType = validationAttribute.GetType();

            string name = char.ToLower(validationAttributeType.Name[0]) + validationAttributeType.Name.Replace("Attribute", "").Substring(1);
            string errorMessage = validationAttribute.ErrorMessageResourceType.GetProperty(validationAttribute.ErrorMessageResourceName).GetValue(null, null) as string;

            List<PropertyInfo> propertyInfos = validationAttributeType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).ToList<PropertyInfo>();

            Dictionary<string, string> options = new Dictionary<string, string>();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                // Use ToString here since value may not be a string
                options.Add(propertyInfo.Name,
                    propertyInfo.GetValue(validationAttribute).ToString());
            }
            return new ValidatorResponseModel() {
                Name = name,
                ErrorMessage = errorMessage,
                Options = options
            };
        }
    }
}
