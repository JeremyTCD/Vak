using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;

namespace Jering.Mail
{
    /// <summary>
    /// Provides an API for handling emails in the development environment.
    /// </summary>
    public class DevelopmentEmailService : EmailService
    {
        /// <summary>
        /// Constructs an instance of <see cref="DevelopmentEmailService"/> .
        /// </summary>
        /// <param name="emailOptionsAccessor"></param>
        /// <param name="smtpClient"></param>
        public DevelopmentEmailService(IOptions<EmailOptions> emailOptionsAccessor) : base(emailOptionsAccessor, null)
        {
        }

#pragma warning disable 1998
        /// <summary>
        /// Writes <paramref name="mimeMessage"/> to a file.
        /// </summary>
        /// <param name="mimeMessage"></param>
        /// <returns></returns>
        public override async Task SendEmailAsync(MimeMessage mimeMessage)
        {
            mimeMessage.WriteTo(_emailOptions.DevelopmentFile);
        }
#pragma warning restore 1998
    }
}
