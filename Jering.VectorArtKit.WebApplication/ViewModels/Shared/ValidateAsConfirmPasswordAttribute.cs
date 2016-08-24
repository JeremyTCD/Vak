using Jering.AccountManagement.Security;
using Jering.VectorArtKit.WebApplication.ViewModels.Shared;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Shared
{
    /// <summary>
    /// Validates a confirm password property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateAsConfirmPasswordAttribute : ValidationAttribute
    {
        private string _passwordProperty { get; set; }

        public ValidateAsConfirmPasswordAttribute(string passwordProperty)
        {
            _passwordProperty = passwordProperty;
        }

        /// <summary>
        /// Validates <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with error message <see cref="ViewModelOptions.ConfirmPassword_DoesNotMatchPassword"/> if password property does not exist.
        /// <see cref="ValidationResult"/> with error message <see cref="ViewModelOptions.ConfirmPassword_DoesNotMatchPassword"/> if <paramref name="value"/> cannot be converted into a string.
        /// <see cref="ValidationResult"/> with error message <see cref="ViewModelOptions.ConfirmPassword_DoesNotMatchPassword"/> if <paramref name="value"/> does not match password property value.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> matches password property value.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ViewModelOptions viewModelOptions = ((IOptions<ViewModelOptions>)validationContext.GetService(typeof(IOptions<ViewModelOptions>))).Value;

            string valueAsString = value as string;
            if (valueAsString == null)
            {
                return new ValidationResult(viewModelOptions.ConfirmPassword_DoesNotMatchPassword);
            }

            PropertyInfo passwordPropertyInfo = validationContext.ObjectType.GetRuntimeProperty(_passwordProperty);
            if (passwordPropertyInfo == null)
            {
                return new ValidationResult(viewModelOptions.ConfirmPassword_DoesNotMatchPassword);
            }

            object otherPropertyValue = passwordPropertyInfo.GetValue(validationContext.ObjectInstance, null);
            if (!Equals(value, otherPropertyValue))
            {
                return new ValidationResult(viewModelOptions.ConfirmPassword_DoesNotMatchPassword);
            }

            return ValidationResult.Success;
        }
    }
}
