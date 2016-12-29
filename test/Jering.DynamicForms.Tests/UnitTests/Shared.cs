using Jering.DataAnnotations;
using Jering.DynamicForms.Tests.Resources;
using System;
using System.ComponentModel.DataAnnotations;

namespace Jering.DynamicForms.Tests.UnitTests
{
    public class DummyRequestModel_NoDynamicFormAttribute
    {

    }

    [DynamicForm(nameof(DummyStrings.Dummy), nameof(DummyStrings.Dummy), typeof(DummyStrings))]
    public class DummyRequestModel
    {
        [ValidateMatches("Password", nameof(DummyStrings.Dummy), typeof(DummyStrings))]
        [AsyncValidate(nameof(DummyStrings.Dummy), typeof(DummyStrings), "TestController", "TestAction")]
        [DynamicControl("input", nameof(DummyStrings.Dummy), typeof(DummyStrings), 0)]
        [DynamicControlProperty("type", "email")]
        [DynamicControlProperty("placeholder", propertyValueResourceName: nameof(DummyStrings.Dummy), resourceType: typeof(DummyStrings))]
        public string Email { get; set; }

        [AsyncValidate(nameof(DummyStrings.Dummy), typeof(DummyStrings), "TestController", "TestAction")]
        [DynamicControl("input", nameof(DummyStrings.Dummy), typeof(DummyStrings), 0)]
        public string SecondaryEmail { get; set; }

        public string Password { get; set; }
    }

    public class DummyValidationAttribute : ValidationAttribute
    {
        public string DummyStringProperty { get; set; }
        public int DummyIntProperty { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resourceType"></param>
        public DummyValidationAttribute(string dummyStringProperty, int dummyIntProperty, string resourceName, Type resourceType)
        {
            DummyStringProperty = dummyStringProperty;
            DummyIntProperty = dummyIntProperty;
            ErrorMessageResourceName = resourceName;
            ErrorMessageResourceType = resourceType;
        }
    }
}
