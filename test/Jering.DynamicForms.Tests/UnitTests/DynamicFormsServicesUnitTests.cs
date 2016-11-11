using Xunit;
using System;
using Moq;
using System.Reflection;
using Jering.DynamicForms.Tests.Resources;
using Jering.DataAnnotations;

namespace Jering.DynamicForms.Tests.UnitTests
{
    public class DynamicFormServicesUnitTests
    {
        [Fact]
        public void GetDynamicForm_ThrowsArgumentExceptionIfViewModelTypeDoesNotHaveDynamicFormAttribute()
        {
            // Arrange
            Mock<IDynamicFormsBuilder> mockBuilder = new Mock<IDynamicFormsBuilder>();
            DynamicFormsService dynamicFormsService = new DynamicFormsService(mockBuilder.Object);

            // Act and assert
            Assert.Throws<ArgumentException>(() => dynamicFormsService.GetDynamicForm(typeof(DummyNoDynamicFormAttributeFormModel)));
        }

        [Fact]
        public void GetDynamicForm_ReturnsDynamicFormDataIfViewModelTypeHasDynamicFormAttribute()
        {
            // Arrange
            Mock<IDynamicFormsBuilder> mockBuilder = new Mock<IDynamicFormsBuilder>();
            mockBuilder.Setup(b => b.BuildDynamicFormData(It.IsAny<DynamicFormAttribute>())).Returns(new DynamicFormData());
            mockBuilder.Setup(b => b.BuildDynamicControlData(It.IsAny<PropertyInfo>())).Returns(new DynamicControlData());
            DynamicFormsService dynamicFormsService = new DynamicFormsService(mockBuilder.Object);

            // Act 
            DynamicFormData result = dynamicFormsService.GetDynamicForm(typeof(DummyFormModel));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.DynamicControlDatas.Count, 2);
            mockBuilder.VerifyAll();
        }

        [DynamicForm(nameof(DummyStrings.Dummy), typeof(DummyStrings))]
        private class DummyFormModel
        {
            [ValidateMatches("Password", nameof(DummyStrings.Dummy), typeof(DummyStrings))]
            [AsyncValidateEmailNotInUse(nameof(DummyStrings.Dummy), typeof(DummyStrings), "TestController", "TestAction")]
            [DynamicControl("input", nameof(DummyStrings.Dummy), typeof(DummyStrings), 0)]
            [DynamicControlProperty("type", "email")]
            [DynamicControlProperty("placeholder", propertyValueResourceName: nameof(DummyStrings.Dummy), resourceType: typeof(DummyStrings))]
            public string Email { get; set; }

            public string Password { get; set; }
        }

        private class DummyNoDynamicFormAttributeFormModel
        {
        }
    }
}
