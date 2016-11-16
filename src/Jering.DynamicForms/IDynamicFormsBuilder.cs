using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DynamicForms
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDynamicFormsBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        DynamicFormResponseModel BuildDynamicFormResponseModel(TypeInfo typeInfo);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        DynamicControlResponseModel BuildDynamicControlResponseModel(PropertyInfo propertyInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validationAttribute"></param>
        /// <returns></returns>
        ValidatorResponseModel BuildDynamicControlValidatorResponseModel(ValidationAttribute validationAttribute);
    }
}
