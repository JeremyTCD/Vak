using Jering.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Reflection;
using Xunit;

namespace Jering.DynamicForms.Tests.UnitTests
{
    public class DynamicFormServiceUnitTests
    {
        [Fact]
        public void GetDynamicFormActionAsync_ReturnsDynamicFormDataIfAModelWithNameModelNameExistsAndHasADynamicFormAttribute()
        {
            // Arrange
            Type testType = typeof(DummyRequestModel);
            string testTypeName = testType.Name;
            DynamicFormData testDynamicFormData = new DynamicFormData();

            Mock<IMemoryCacheService> mockMemoryCacheService = new Mock<IMemoryCacheService>();
            mockMemoryCacheService.Setup(m => m.Get(It.Is<string>(s => s == testTypeName))).Returns(null);
            mockMemoryCacheService.
                Setup(m => m.Set(It.Is<string>(s => s == testTypeName),
                    It.Is<DynamicFormData>(d => d == testDynamicFormData),
                    It.Is<MemoryCacheEntryOptions>(mc => mc.Priority == CacheItemPriority.NeverRemove)));

            Mock<Assembly> mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(a => a.ExportedTypes).Returns(new Type[] { testType });

            Mock<IAssemblyService> mockAssemblyService = new Mock<IAssemblyService>();
            mockAssemblyService.Setup(a => a.GetEntryAssembly()).Returns(mockAssembly.Object);

            Mock<IDynamicFormBuilder> mockDynamicFormBuilder = new Mock<IDynamicFormBuilder>();
            mockDynamicFormBuilder.
                Setup(d => d.BuildDynamicFormData(It.Is<TypeInfo>(ti => ti == testType.GetTypeInfo()))).
                Returns(testDynamicFormData);

            DynamicFormService dynamicFormService = new DynamicFormService(mockDynamicFormBuilder.Object, 
                mockAssemblyService.Object,
                mockMemoryCacheService.Object);

            // Act
            DynamicFormData result = dynamicFormService.GetDynamicFormAction(testTypeName);

            // Assert
            Assert.NotNull(result);
            mockMemoryCacheService.VerifyAll();
            mockAssembly.VerifyAll();
            mockAssemblyService.VerifyAll();
            mockDynamicFormBuilder.VerifyAll();
        }
    }
}
