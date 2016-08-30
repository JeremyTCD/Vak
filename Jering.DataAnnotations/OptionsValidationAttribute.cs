using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// 
    /// </summary>
    public class OptionsValidationAttribute : ValidationAttribute
    {
        private string _errorMessagePropertyName { get; set; }
        private Type _optionsType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMessagePropertyName"></param>
        /// <param name="optionsType"></param>
        protected OptionsValidationAttribute(string errorMessagePropertyName, Type optionsType)
        {
            _errorMessagePropertyName = errorMessagePropertyName;
            _optionsType = optionsType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected string GetErrorMessage(ValidationContext validationContext)
        {
            object options = ((IOptions<object>)validationContext.
                GetService(typeof(IOptions<>).MakeGenericType(_optionsType))).
                Value;

            return options.GetType().GetProperty(_errorMessagePropertyName).GetValue(options) as string;
        }
    }
}
