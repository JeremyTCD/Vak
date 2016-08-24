using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApplication.ViewModels.Shared
{
    /// <summary>
    /// Validates an email address's format.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateAsEmailAddressAttribute : ValidationAttribute
    {
        /// <summary>
        /// Validates <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with error message <see cref="ViewModelOptions.Email_Invalid"/> if <paramref name="value"/> cannot be converted into a string.
        /// <see cref="ValidationResult"/> with error message <see cref="ViewModelOptions.Email_Invalid"/> if <paramref name="value"/> isn't formatted as an email.
        /// <see cref="ValidationResult.Success"/> if email address is properly formatted.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ViewModelOptions viewModelOptions = ((IOptions<ViewModelOptions>)validationContext.GetService(typeof(IOptions<ViewModelOptions>))).Value;

            string valueAsString = value as string;
            if (valueAsString == null)
            {
                return new ValidationResult(viewModelOptions.Email_Invalid);
            }

            // only return true if there is only 1 '@' character
            // and it is neither the first nor the last character
            bool found = false;
            for (int i = 0; i < valueAsString.Length; i++)
            {
                if (valueAsString[i] == '@')
                {
                    if (found || i == 0 || i == valueAsString.Length - 1)
                    {
                        return new ValidationResult(viewModelOptions.Email_Invalid);
                    }
                    found = true;
                }
            }

            return found ? ValidationResult.Success : new ValidationResult(viewModelOptions.Email_Invalid);
        }
    }
}
