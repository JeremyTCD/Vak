using Xunit;
using System;
using Moq;
using System.Reflection;

namespace Jering.DynamicForms.Tests.UnitTests
{
    public class DynamicFormServicesUnitTests
    {
        [Fact]
        public void GetDynamicForm_ThrowsArgumentExceptionIfViewModelTypeDoesNotHaveDynamicFormAttribute()
        {
            // Arrange
            Mock<IDynamicFormsBuilder> mockBuilder = new Mock<IDynamicFormsBuilder>();
            DynamicFormsServices dynamicFormsServices = new DynamicFormsServices(mockBuilder.Object);

            // Act and assert
            Assert.Throws<ArgumentException>(() => dynamicFormsServices.GetDynamicForm(typeof(DummyNoDynamicFormAttributeFormModel)));
        }

        [Fact]
        public void GetDynamicForm_ReturnsDynamicFormDataIfViewModelTypeHasDynamicFormAttribute()
        {
            // Arrange
            Mock<IDynamicFormsBuilder> mockBuilder = new Mock<IDynamicFormsBuilder>();
            mockBuilder.Setup(b => b.BuildDynamicFormData(It.IsAny<DynamicFormAttribute>())).Returns(new DynamicFormData());
            mockBuilder.Setup(b => b.BuildDynamicControlData(It.IsAny<PropertyInfo>())).Returns(new DynamicControlData());
            DynamicFormsServices dynamicFormsServices = new DynamicFormsServices(mockBuilder.Object);

            // Act 
            DynamicFormData result = dynamicFormsServices.GetDynamicForm(typeof(DummyFormModel));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.DynamicControlDatas.Count, 2);
            mockBuilder.VerifyAll();
        }
    }

    public class DummyNoDynamicFormAttributeFormModel
    {
    }
}
