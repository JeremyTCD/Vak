using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
        /// <param name="dynamicFormAttribute"></param>
        /// <returns></returns>
        DynamicFormData BuildDynamicFormData(DynamicFormAttribute dynamicFormAttribute);


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
        DynamicControlValidatorData BuildDynamicControlValidatorData(ValidationAttribute validationAttribute);
    }
}
