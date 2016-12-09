// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;
using System.Collections.Generic;

namespace Jering.AccountManagement.Security.Tests.UnitTests
{
    public class PasswordServiceTest
    {
        private PasswordServiceOptions _testPasswordServiceOptions;
        private const string _testPassword = "my password";
        private const string _testInvalidPassword = "testInvalidPassword";

        public PasswordServiceTest()
        {
            _testPasswordServiceOptions = new PasswordServiceOptions { Rng = new SequentialRandomNumberGenerator()};
        }

        [Theory]
        [MemberData(nameof(ConstructorThrowsInvalidOperationExceptionData))]
        public void Constructor_ThrowsInvalidOperationExceptionIfIterationCountIsNotAPositiveInteger(int iterationCount)
        {
            // Arrange
            _testPasswordServiceOptions.IterationCount = iterationCount;
            Mock<IOptions<PasswordServiceOptions>> mockOptionsAccessor = CreateMockPasswordServiceOptionsAccessor();

            // Act & assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                new PasswordService(mockOptionsAccessor.Object);
            });
            mockOptionsAccessor.VerifyAll();
        }

        public static IEnumerable<object[]> ConstructorThrowsInvalidOperationExceptionData()
        {
            yield return new object[] { -1 };
            yield return new object[] { 0 };
        }

        [Fact]
        public void HashPassword_ReturnsExpectedPasswordHash()
        {
            // Arrange
            PasswordService passwordService = new PasswordService(CreateMockPasswordServiceOptionsAccessor().Object);

            // Act
            string passwordHash = passwordService.HashPassword(_testPassword);

            // Assert
            Assert.Equal("AQAAAAEAACcQAAAAEAABAgMEBQYHCAkKCwwNDg+yWU7rLgUwPZb1Itsmra7cbxw2EFpwpVFIEtP+JIuUEw==", passwordHash);
        }

        [Fact]
        public void VerifyPassword_ReturnsTrueIfPasswordMatchesPasswordUsedToGeneratePasswordHash()
        {
            // Arrange
            PasswordService passwordService = new PasswordService(CreateMockPasswordServiceOptionsAccessor().Object);
            string passwordHash = passwordService.HashPassword(_testPassword);

            // Assert
            bool result = passwordService.VerifyPassword(passwordHash, _testPassword);

            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ReturnsFalseIfPasswordDoesNotMatchPasswordUsedToGeneratePasswordHash()
        {
            // Arrange
            PasswordService passwordService = new PasswordService(CreateMockPasswordServiceOptionsAccessor().Object);
            string passwordHash = passwordService.HashPassword(_testPassword);

            // Assert
            bool result = passwordService.VerifyPassword(passwordHash, _testInvalidPassword);

            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(VerifyPasswordSuccessCasesData))]
        public void VerifyHashedPassword_SuccessCases(string passwordHash)
        {
            // Arrange
            PasswordService passwordService = new PasswordService(CreateMockPasswordServiceOptionsAccessor().Object);

            // Act
            bool result = passwordService.VerifyPassword(passwordHash,_testPassword);

            // Assert
            Assert.True(result);
        }

        // TODO add cases for larger salt/subkey
        public static IEnumerable<object[]> VerifyPasswordSuccessCasesData()
        {
            // SHA256, 250000 iterations, 256-bit salt, 256-bit subkey
            yield return new object[] { "AQAAAAEAA9CQAAAAIESkQuj2Du8Y+kbc5lcN/W/3NiAZFEm11P27nrSN5/tId+bR1SwV8CO1Jd72r4C08OLvplNlCDc3oQZ8efcW+jQ=" };
        }

        [Theory]
        [MemberData(nameof(VerifyPasswordFailureCasesData))]
        public void VerifyPassword_FailureCases(string passwordHash)
        {
            // Arrange
            PasswordService passwordService = new PasswordService(CreateMockPasswordServiceOptionsAccessor().Object);

            // Act
            bool result = passwordService.VerifyPassword(passwordHash, _testPassword);

            // Assert
            Assert.False(result);
        }

        public static IEnumerable<object[]> VerifyPasswordFailureCasesData()
        {
            // incorrect password
            yield return new object[] { "AQAAAAAAAAD6AAAAEAhftMyfTJyAAAAAAAAAAAAAAAAAAAih5WsjXaR3PA9M" };
            // too short
            yield return new object[] { "AQAAAAIAAAAyAAAAEOMwvh3+FZxqkdMBz2ekgGhwQ4A=" };
            // extra data at end
            yield return new object[] { "AQAAAAIAAAAyAAAAEOMwvh3+FZxqkdMBz2ekgGhwQ4B6pZWND6zgESBuWiHwAAAAAAAAAAAA" };
        }

        #region Helpers
        private sealed class SequentialRandomNumberGenerator : RandomNumberGenerator
        {
            private byte _value;

            public override void GetBytes(byte[] data)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = _value++;
                }
            }
        }

        private Mock<IOptions<PasswordServiceOptions>> CreateMockPasswordServiceOptionsAccessor()
        {
            Mock<IOptions<PasswordServiceOptions>> mockOptions = new Mock<IOptions<PasswordServiceOptions>>();
            mockOptions.
                Setup(o => o.Value).
                Returns(_testPasswordServiceOptions);

            return mockOptions;
        }
        #endregion
    }
}