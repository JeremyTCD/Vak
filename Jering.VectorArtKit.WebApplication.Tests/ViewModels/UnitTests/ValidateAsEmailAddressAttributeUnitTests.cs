using Jering.AccountManagement.Security;
using Jering.VectorArtKit.WebApplication.ViewModels.Shared;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace Jering.VectorArtKit.WebApplication.Tests.ViewModels.UnitTests
{
    public class ValidateAsEmailAddressAttributeUnitTests
    {
        [Theory]
        [MemberData(nameof(IsValidData))]
        public void IsValid_GetValidationResult_ReturnsCorrectValidationResults(object dummyEmail, string outputString)
        {
            // Arrange
            Mock<IOptions<ViewModelOptions>> mockViewModelOptions = new Mock<IOptions<ViewModelOptions>>();
            mockViewModelOptions.Setup(o => o.Value).Returns(new ViewModelOptions());

            Mock<IServiceProvider> mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetService(typeof(IOptions<ViewModelOptions>))).Returns(mockViewModelOptions.Object);

            ValidationContext validationContext = new ValidationContext(new { }, mockServiceProvider.Object, null);

            ValidateAsEmailAddressAttribute validateAsEmailAddressAttribute = new ValidateAsEmailAddressAttribute();

            // Act
            // IsValid is a protected function, the public function GetValidationResult calls it.
            ValidationResult validationResult = validateAsEmailAddressAttribute.GetValidationResult(dummyEmail, validationContext);

            // Assert
            if(dummyEmail as string == "test@domain.com")
            {
                Assert.Null(validationResult);
            }
            else
            {
                Assert.Equal(outputString, validationResult.ErrorMessage);
            }           
        }

        public static IEnumerable<object[]> IsValidData ()
        {
            ViewModelOptions viewModelOptions = new ViewModelOptions();

            yield return new object[] { "@test", viewModelOptions.Email_Invalid };
            yield return new object[] { "test@", viewModelOptions.Email_Invalid };
            yield return new object[] { "test", viewModelOptions.Email_Invalid };
            yield return new object[] { 0, viewModelOptions.Email_Invalid };
            yield return new object[] { "test@domain.com", null };
        }
    }
}
