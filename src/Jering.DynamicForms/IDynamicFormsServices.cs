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
        /// <see cref="DynamicForm"/> equivalent of <paramref name="viewModelType"/>.
        /// </returns>
        DynamicForm GetToDynamicForm(Type viewModelType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns>
        /// <see cref="DynamicInput"/> equivalent of <paramref name="propertyInfo"/> if <paramref name="propertyInfo"/> contains <see cref="DynamicInputAttribute"/>.
        /// Otherwise, null.
        /// </returns>
        DynamicInput ConvertToDynamicInput(PropertyInfo propertyInfo);
    }
}
