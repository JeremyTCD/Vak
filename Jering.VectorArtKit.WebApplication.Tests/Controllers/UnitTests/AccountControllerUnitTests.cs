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
        // TODO : move all these to integration tests to test whether model validation is setup correctly

        [Fact]
        public async Task LoginPost_ReturnsLoginViewIfModelStateIsInvalid()
        {
            // Arrange
            AccountController accountController = new AccountController(null, null);
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

            Mock<IAccountSecurityServices<VakAccount>> mockAccountSecurityServices = new Mock<IAccountSecurityServices<VakAccount>>();
            mockAccountSecurityServices.Setup(a => a.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>())).ReturnsAsync(PasswordSignInResult.Failed);
            AccountController accountController = new AccountController(null, mockAccountSecurityServices.Object);

            // Act
            ViewResult viewResult = (ViewResult)await accountController.Login(loginViewModel, null);

            // Assert
            Assert.IsType<ViewResult>(viewResult);
            Assert.Null(viewResult.ViewName);
            Assert.Equal("Invalid email or password.", viewResult.ViewData.ModelState.Values.First().Errors.First().ErrorMessage);
            mockAccountSecurityServices.VerifyAll();
        }

        [Fact]
        public async Task RegisterPost_ReturnsRegisterViewIfModelStateIsInvalid()
        {
            // Arrange
            AccountController accountController = new AccountController(null, null);
            accountController.ModelState.AddModelError("", "");

            // Act
            ViewResult viewResult = (ViewResult)await accountController.Register(null, null);

            // Assert
            Assert.IsType<ViewResult>(viewResult);
            // Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ViewResultExecutor replaces null with the name of the action that returned the ViewResult.
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task RegisterPost_ReturnsRegisterViewAndErrorMessageIfEmailIsInvalid()
        {
            // Arrange
            RegisterViewModel registerViewModel = new RegisterViewModel() { Email = "Email1", Password = "Password1", ConfirmPassword = "Password1" };

            Mock<IAccountSecurityServices<VakAccount>> mockAccountSecurityServices = new Mock<IAccountSecurityServices<VakAccount>>();
            mockAccountSecurityServices.Setup(a => a.CreateAccountAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(CreateAccountResult.Failed);
            AccountController accountController = new AccountController(null, mockAccountSecurityServices.Object);

            // Act
            ViewResult viewResult = (ViewResult)await accountController.Register(registerViewModel, null);

            // Assert
            Assert.IsType<ViewResult>(viewResult);
            Assert.Null(viewResult.ViewName);
            Assert.Equal("An account with this email already exists.", viewResult.ViewData.ModelState.Values.First().Errors.First().ErrorMessage);
            mockAccountSecurityServices.VerifyAll();
        }
    }
}
