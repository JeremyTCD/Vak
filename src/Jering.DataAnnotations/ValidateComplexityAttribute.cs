using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Jering.DataAnnotations
{
    /// <summary>
    /// Validates that a string is sufficiently complex
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateComplexityAttribute : ValidationAttribute
    {
        private static char[] NonAlphanumericChars = new char[] { '(', ')', '`', '~','!', '@', '#', '$', '%', '^', '&', '*', '-', '+',
                '=', '|', '\\', '{', '}', '[', ']', ':', ';', '"',
                '\'', '<', '>', ',', '.', '?', '/', '_' };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public ValidateComplexityAttribute(string resourceName, Type resourceType) 
        {
            ErrorMessageResourceName = resourceName;
            ErrorMessageResourceType = resourceType;
        }

        /// <summary>
        /// Validates <paramref name="value"/>. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>
        /// <see cref="ValidationResult"/> with an error message if <paramref name="value"/> is not sufficiently complex.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> is sufficiently complex.
        /// <see cref="ValidationResult.Success"/> if <paramref name="value"/> is null or an empty string.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string valueAsString = value as string;
            if (valueAsString == null || valueAsString.Trim().Length == 0)
            {
                return ValidationResult.Success;
            }

            int numPossibilitiesPerChar = 0;
            bool hasLower = false, hasUpper = false, hasDigit = false, hasNonAlphanumeric = false;


            foreach (char c in valueAsString)
            {
                if (!hasLower && IsLower(c))
                {
                    numPossibilitiesPerChar += 26;
                    hasLower = true;
                }

                if (!hasUpper && IsUpper(c))
                {
                    numPossibilitiesPerChar += 26;
                    hasUpper = true;
                }

                if (!hasDigit && IsDigit(c))
                {
                    numPossibilitiesPerChar += 10;
                    hasDigit = true;
                }

                if (!hasNonAlphanumeric && IsNonAlphaNumeric(c))
                {
                    numPossibilitiesPerChar += 32;
                    hasNonAlphanumeric = true;
                }
            }

            if (Math.Pow(numPossibilitiesPerChar, valueAsString.Length) < 2.8E12)
            {
                return new ValidationResult(ErrorMessageString);
            }

            return ValidationResult.Success;
        }

        private bool IsLower(char c)
        {
            return c >= 'a' && c <= 'z';
        }

        private bool IsUpper(char c)
        {
            return c >= 'A' && c <= 'Z';
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool IsNonAlphaNumeric(char c)
        {
            return NonAlphanumericChars.Contains(c);
        }
    }
}
