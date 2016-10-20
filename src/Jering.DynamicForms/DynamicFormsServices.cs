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
        /// Constructor
        /// </summary>
        public DynamicFormsServices()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <returns>
        /// <see cref="DynamicFormData"/> equivalent of <paramref name="viewModelType"/>.
        /// </returns>
        public DynamicFormData GetDynamicForm(Type viewModelType)
        {
            PropertyInfo[] propertyInfos = viewModelType.GetProperties();
            DynamicFormData dynamicForm = new DynamicFormData();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                DynamicControlData dynamicControl = DynamicControlData.FromPropertyInfo(propertyInfo);
                if (dynamicControl != null)
                    dynamicForm.AddDynamicControl(dynamicControl);
            }

            return dynamicForm;
        }
    }
}
