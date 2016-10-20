using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DynamicForms
{
    /// <summary>
    /// Defines a dynamic control. Corresponds to javascript dynamic control type.
    /// </summary>
    public class DynamicControlData
    {
        /// <summary>
        /// Control name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Control Html tag name
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// Position of control relative to other controls in same dynamic form
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Display name for control. Rendered as text content of associated label.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Html element properties
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// Control validators data
        /// </summary>
        public List<DynamicControlValidatorData> ValidatorDatas { get; set; }

        /// <summary>
        /// Creates a <see cref="DynamicControlData"/> from <paramref name="propertyInfo"/>.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns>
        /// <see cref="DynamicControlData"/> equivalent of <paramref name="propertyInfo"/> if <paramref name="propertyInfo"/> contains <see cref="DynamicControlAttribute"/>.
        /// Otherwise, null.
        /// </returns>
        public static DynamicControlData FromPropertyInfo(PropertyInfo propertyInfo)
        {
            DynamicControlAttribute dynamicControlAttribute = propertyInfo.GetCustomAttribute(typeof(DynamicControlAttribute)) as DynamicControlAttribute;

            if (dynamicControlAttribute != null)
            {
                IEnumerable<ValidationAttribute> validationAttributes = propertyInfo.GetCustomAttributes<ValidationAttribute>();
                List<DynamicControlValidatorData> validators = new List<DynamicControlValidatorData>();
                foreach (ValidationAttribute validationAttribute in validationAttributes)
                {
                    validators.Add(DynamicControlValidatorData.FromValidationAttribute(validationAttribute));
                }

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
                    ValidatorDatas = validators,
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
    }
}
