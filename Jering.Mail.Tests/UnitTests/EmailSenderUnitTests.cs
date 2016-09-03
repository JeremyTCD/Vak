using Jering.Mail;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jering.AccountManagement.Security.UnitTests.UnitTests
{
    public class EmailSenderUnitTests
    {
        [Fact]
        public void CreateMimeMessage_GeneratesMimeMessageCorrectly()
        {
            // Arrange
            string emailAddress = "test@email.com";
            string subject = "subject";
            string message = "message";

            EmailOptions emailOptions = new EmailOptions();

            Mock<IOptions<EmailOptions>> mockOptions = new Mock<IOptions<EmailOptions>>();
            mockOptions.Setup(o => o.Value).Returns(emailOptions);

            EmailServices emailSender = new EmailServices(mockOptions.Object, null);

            // Act
            MimeMessage mimeMessage = emailSender.CreateMimeMessage(message, emailAddress, subject);

            // Assert
            mockOptions.VerifyAll();
            Assert.Equal(message, mimeMessage.TextBody);
            Assert.Equal(subject, mimeMessage.Subject);
            Assert.Equal($"\"{emailAddress}\" <{emailAddress}>", mimeMessage.To.ToString());
            Assert.Equal($"\"{emailOptions.Name}\" <{emailOptions.EmailAddress}>", mimeMessage.From.ToString());
        }
    }
}
