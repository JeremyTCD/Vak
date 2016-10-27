using Jering.DataAnnotations;
using Jering.DynamicForms.Tests.Resources;
using System;
using System.ComponentModel.DataAnnotations;

namespace Jering.DynamicForms.Tests.UnitTests
{
    [DynamicForm(nameof(DummyStrings.Dummy), typeof(DummyStrings))]
    public class DummyFormModel
    {
        [ValidateMatches("Password", nameof(DummyStrings.Dummy), typeof(DummyStrings))]
        [AsyncValidateEmailNotInUse(nameof(DummyStrings.Dummy), typeof(DummyStrings), "TestController", "TestAction")]
        [DynamicControl("input", nameof(DummyStrings.Dummy), typeof(DummyStrings), 0)]
        [DynamicControlProperty("type", "email")]
        [DynamicControlProperty("placeholder", propertyValueResourceName: nameof(DummyStrings.Dummy), resourceType: typeof(DummyStrings))]
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
