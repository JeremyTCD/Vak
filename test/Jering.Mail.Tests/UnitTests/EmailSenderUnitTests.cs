using Jering.Mail;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;
using Xunit;

namespace Jering.Mail.Tests.UnitTests
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

            EmailService emailSender = new EmailService(mockOptions.Object, null);

            // Act
            MimeMessage mimeMessage = emailSender.CreateMimeMessage(emailAddress, subject, message);

            // Assert
            mockOptions.VerifyAll();
            Assert.Equal(message, mimeMessage.TextBody);
            Assert.Equal(subject, mimeMessage.Subject);
            Assert.Equal($"\"{emailAddress}\" <{emailAddress}>", mimeMessage.To.ToString());
            Assert.Equal($"\"{emailOptions.Name}\" <{emailOptions.EmailAddress}>", mimeMessage.From.ToString());
        }
    }
}
