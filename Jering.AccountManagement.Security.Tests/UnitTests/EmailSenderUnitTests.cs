using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jering.AccountManagement.Security.UnitTests.UnitTests
{
    public class EmailSenderUnitTests
    {
        [Fact]
        public async Task SendEmailAsync_GeneratesEmailCorrectlyAndSendsItToCorrectServer()
        {
            // Arrange
            string emailAddress = "test@email.com";
            string subject = "subject";
            string message = "message";

            EmailOptions emailOptions = new EmailOptions();

            Mock<IOptions<EmailOptions>> mockOptions = new Mock<IOptions<EmailOptions>>();
            mockOptions.Setup(o => o.Value).Returns(emailOptions);

            Mock<SmtpClient> mockSmtpClient = new Mock<SmtpClient>();
            mockSmtpClient.Setup(s => s.AuthenticationMechanisms).Returns(new HashSet<string>());
            mockSmtpClient.Setup(s => s.Disconnect(true, It.IsAny<CancellationToken>()));
            mockSmtpClient.Setup(s => s.SendAsync(It.Is<MimeMessage>(m => m.TextBody == message &&
                    m.Subject == subject && 
                    m.To.ToString() == $"\"{emailAddress}\" <{emailAddress}>" &&
                    m.From.ToString() == $"\"{emailOptions.Name}\" <{emailOptions.EmailAddress}>"),
                It.IsAny<CancellationToken>(),
                It.IsAny<ITransferProgress>())).Returns(Task.CompletedTask);

            EmailSender emailSender = new EmailSender(mockOptions.Object, mockSmtpClient.Object);

            // Act
            await emailSender.SendEmailAsync(message, emailAddress, subject);

            // Assert
            mockOptions.VerifyAll();
            mockSmtpClient.VerifyAll();
        }
    }
}
