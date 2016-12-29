using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DynamicForms
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDynamicFormBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        DynamicFormData BuildDynamicFormData(TypeInfo typeInfo);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        DynamicControlData BuildDynamicControlData(PropertyInfo propertyInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validationAttribute"></param>
        /// <returns></returns>
        ValidatorData BuildDynamicControlValidatorData(ValidationAttribute validationAttribute);
    }
}
