using Jering.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DynamicForms
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicFormsServices : IDynamicFormsServices
    {
        /// <summary>
        /// 
        /// </summary>
        public DynamicFormsServices()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <returns>
        /// <see cref="DynamicForm"/> equivalent of <paramref name="viewModelType"/>.
        /// </returns>
        public DynamicForm GetToDynamicForm(Type viewModelType)
        {
            PropertyInfo[] propertyInfos = viewModelType.GetProperties();
            DynamicForm dynamicForm = new DynamicForm();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                DynamicInput dynamicInput = ConvertToDynamicInput(propertyInfo);
                if (dynamicInput != null)
                    dynamicForm.AddDynamicInput(dynamicInput);
            }

            return dynamicForm;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns>
        /// <see cref="DynamicInput"/> equivalent of <paramref name="propertyInfo"/> if <paramref name="propertyInfo"/> contains <see cref="DynamicInputAttribute"/>.
        /// Otherwise, null.
        /// </returns>
        public DynamicInput ConvertToDynamicInput(PropertyInfo propertyInfo)
        {
            DynamicInputAttribute dynamicInputAttribute = propertyInfo.GetCustomAttribute(typeof(DynamicInputAttribute)) as DynamicInputAttribute;

            if (dynamicInputAttribute != null)
            {
                IEnumerable<ValidationAttribute> validationAttributes = propertyInfo.GetCustomAttributes<ValidationAttribute>();
                List<DynamicInputValidatorData> validators = new List<DynamicInputValidatorData>();
                foreach (ValidationAttribute validationAttribute in validationAttributes)
                {
                    validators.Add(DynamicInputValidatorData.FromValidationAttribute(validationAttribute));
                }

                TypeInfo resourceTypeInfo = dynamicInputAttribute.
                        ResourceType.
                        GetTypeInfo();

                return new DynamicInput()
                {
                    Name = propertyInfo.Name,
                    RenderLabel = dynamicInputAttribute.RenderLabel,
                    Type = dynamicInputAttribute.Type,
                    Order = dynamicInputAttribute.Order,
                    TagName = dynamicInputAttribute.TagName,
                    ValidatorData = validators,
                    // Pass null to GetValue since we are retreiving static properties
                    DisplayName = resourceTypeInfo.GetDeclaredProperty(dynamicInputAttribute.DisplayNameResourceName).GetValue(null) as string,
                    Placeholder = dynamicInputAttribute.PlaceholderResourceName == null ? null : resourceTypeInfo.GetDeclaredProperty(dynamicInputAttribute.PlaceholderResourceName).GetValue(null) as string,
                    InitialValue = dynamicInputAttribute.InitialValueResourceName == null ? null : resourceTypeInfo.GetDeclaredProperty(dynamicInputAttribute.InitialValueResourceName).GetValue(null) as string
                };
            }

            return null;
        }
    }
}
