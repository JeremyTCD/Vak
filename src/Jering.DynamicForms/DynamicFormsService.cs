﻿using System;
using System.Reflection;

namespace Jering.DynamicForms
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicFormsService : IDynamicFormsService
    {
        private IDynamicFormsBuilder _dynamicFormsBuilder;
        /// <summary>
        /// Generic form name.
        /// </summary>
        public static string DynamicFormName = "DynamicForm";

        /// <summary>
        /// Constructor
        /// </summary>
        public DynamicFormsService(IDynamicFormsBuilder dynamicFormsBuilder)
        {
            _dynamicFormsBuilder = dynamicFormsBuilder;
        }

        /// <summary>
        /// Generates and returns <see cref="DynamicFormData"/> equivalent of <paramref name="viewModelType"/>.
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <returns>
        /// <see cref="DynamicFormData"/> equivalent of <paramref name="viewModelType"/>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="viewModelType"/> does not have a <see cref="DynamicFormAttribute"/></exception>
        public DynamicFormData GetDynamicForm(Type viewModelType)
        {
            DynamicFormAttribute dynamicFormAttribute = viewModelType.GetTypeInfo().GetCustomAttribute<DynamicFormAttribute>();
            if(dynamicFormAttribute == null)
            {
                throw new ArgumentException(nameof(viewModelType));
            }
            DynamicFormData dynamicFormData = _dynamicFormsBuilder.BuildDynamicFormData(dynamicFormAttribute);

            PropertyInfo[] propertyInfos = viewModelType.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                DynamicControlData dynamicControl = _dynamicFormsBuilder.BuildDynamicControlData(propertyInfo);
                if (dynamicControl != null)
                    dynamicFormData.AddDynamicControl(dynamicControl);
            }

            return dynamicFormData;
        }
    }
}
