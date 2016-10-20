using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;

namespace Jering.DynamicForms
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDynamicFormsServices
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <returns>
        /// <see cref="DynamicFormData"/> equivalent of <paramref name="viewModelType"/>.
        /// </returns>
        DynamicFormData GetDynamicForm(Type viewModelType);
    }
}
