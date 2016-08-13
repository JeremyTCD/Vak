using Jering.AccountManagement.DatabaseInterface;
using Jering.AccountManagement.Security;
using Jering.VectorArtKit.WebApplication.BusinessModel;
using Jering.VectorArtKit.WebApplication.Controllers;
using Jering.VectorArtKit.WebApplication.ViewModels;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Jering.VectorArtKit.WebApplication.Tests.Controllers.UnitTests
{
    public class AccountControllerUnitTests
    {
        [Fact]
        public async Task LoginPost_ReturnsLoginViewIfModelStateIsInvalid()
        {
            // Arrange
            Mock<VakAccountRepository> mockVakAccountRepository = new Mock<VakAccountRepository>(new SqlConnection());
            Mock<IAccountSecurityServices<VakAccount>> mockAccountSecurityServices = new Mock<IAccountSecurityServices<VakAccount>>();
            AccountController accountController = new AccountController(mockVakAccountRepository.Object, mockAccountSecurityServices.Object);
            accountController.ModelState.AddModelError("", "");

            // Act
            ViewResult viewResult = (ViewResult) await accountController.Login(null, null);

            // Assert
            Assert.IsType<ViewResult>(viewResult);
            // Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor replaces null with the name of the action that returned the ViewResult.
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task LoginPost_ReturnsLoginViewAndErrorMessageIfCredentialsAreInvalid()
        {
            // Arrange
            LoginViewModel loginViewModel = new LoginViewModel() { Email = "Email1", Password = "Password1", RememberMe = true };

            Mock<VakAccountRepository> mockVakAccountRepository = new Mock<VakAccountRepository>(new SqlConnection());
            Mock<IAccountSecurityServices<VakAccount>> mockAccountSecurityServices = new Mock<IAccountSecurityServices<VakAccount>>();
            mockAccountSecurityServices.Setup(a => a.ApplicationPasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>())).ReturnsAsync(false);
            AccountController accountController = new AccountController(mockVakAccountRepository.Object, mockAccountSecurityServices.Object);

            // Act
            ViewResult viewResult = (ViewResult)await accountController.Login(loginViewModel, null);

            // Assert
            Assert.IsType<ViewResult>(viewResult);
            Assert.Null(viewResult.ViewName);
            Assert.Equal("Invalid login attempt.", viewResult.ViewData.ModelState.Values.First().Errors.First().ErrorMessage);
        }
    }
}
