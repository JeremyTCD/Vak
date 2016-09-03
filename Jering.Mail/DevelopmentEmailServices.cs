using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Mail
{
    /// <summary>
    /// Provides an API for handling emails in the development environment.
    /// </summary>
    public class DevelopmentEmailServices : EmailServices
    {
        /// <summary>
        /// Constructs an instance of <see cref="DevelopmentEmailServices"/> .
        /// </summary>
        /// <param name="emailOptionsAccessor"></param>
        /// <param name="smtpClient"></param>
        public DevelopmentEmailServices(IOptions<EmailOptions> emailOptionsAccessor) : base(emailOptionsAccessor, null)
        {
        }

        /// <summary>
        /// Writes <paramref name="mimeMessage"/> to a file.
        /// </summary>
        /// <param name="mimeMessage"></param>
        /// <returns></returns>
        public override async Task SendEmailAsync(MimeMessage mimeMessage)
        {
            mimeMessage.WriteTo(_emailOptions.DevelopmentFile);
        }
    }
}
