using Jering.Utilities;
using Moq;
using System;
using System.Reflection;
using Xunit;

namespace Jering.DynamicForms.Tests.UnitTests
{
    public class DynamicFormServiceUnitTests
    {
        private Type _testType { get; } = typeof(DummyRequestModel);
        private string _testTypeName { get; } = typeof(DummyRequestModel).Name;
        private string _testInvalidTypeName { get; } = "testInvalidTypeName";
        private string _testAssemblyName { get; } = typeof(DynamicFormService).GetTypeInfo().Assembly.GetName().Name;
        private DynamicFormData _testDynamicFormData { get; } = new DynamicFormData();

        [Fact]
        public void GetDynamicFormActionAsync_ReturnsDynamicFormDataIfAModelWithNameModelNameExistsAndHasADynamicFormAttribute()
        {
            // Arrange
            Mock<Assembly> mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.ExportedTypes).Returns(new Type[] { _testType });

            Mock<IAssemblyService> mockAssemblyService = new Mock<IAssemblyService>();
            mockAssemblyService.
                Setup(a => a.GetReferencingAssemblies(It.Is<string>(s => s == _testAssemblyName))).
                Returns(new Assembly[] { mockAssembly.Object });

            Mock<IDynamicFormBuilder> mockDynamicFormBuilder = new Mock<IDynamicFormBuilder>();
            mockDynamicFormBuilder.
                Setup(d => d.BuildDynamicFormData(It.Is<TypeInfo>(ti => ti == _testType.GetTypeInfo()))).
                Returns(_testDynamicFormData);

            DynamicFormService dynamicFormService = new DynamicFormService(mockDynamicFormBuilder.Object, 
                mockAssemblyService.Object);

            // Act
            DynamicFormData result = dynamicFormService.GetDynamicFormAction(_testTypeName);

            // Assert
            mockAssembly.VerifyAll();
            mockAssemblyService.VerifyAll();
            mockDynamicFormBuilder.VerifyAll();
            Assert.NotNull(result);
        }

        [Fact]
        public void GetDynamicFormActionAsync_ReturnsNullIfAModelWithNameModelNameDoesNotExist()
        {
            // Arrange
            Mock<Assembly> mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.ExportedTypes).Returns(new Type[] { _testType });

            Mock<IAssemblyService> mockAssemblyService = new Mock<IAssemblyService>();
            mockAssemblyService.
                Setup(a => a.GetReferencingAssemblies(It.Is<string>(s => s == _testAssemblyName))).
                Returns(new Assembly[] { mockAssembly.Object });

            DynamicFormService dynamicFormService = new DynamicFormService(null,
                mockAssemblyService.Object);

            // Act
            DynamicFormData result = dynamicFormService.GetDynamicFormAction(_testInvalidTypeName);

            // Assert
            mockAssembly.VerifyAll();
            mockAssemblyService.VerifyAll();
            Assert.Null(result);
        }
    }
}
