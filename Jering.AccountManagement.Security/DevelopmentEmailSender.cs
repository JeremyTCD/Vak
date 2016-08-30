using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// Provides an API for handling emails in the development environment.
    /// </summary>
    public class DevelopmentEmailSender : IEmailSender
    {
       private EmailOptions _emailOptions { get; }

        /// <summary>
        /// Constructs and instance of <see cref="DevelopmentEmailSender"/>. 
        /// </summary>
        /// <param name="emailOptionsAccessor"></param>
        public DevelopmentEmailSender(IOptions<EmailOptions> emailOptionsAccessor)
        {
            _emailOptions = emailOptionsAccessor.Value;
        }

        /// <summary>
        /// Writes an email with message <paramref name="message"/> and subject <paramref name="subject"/> to the 
        /// file <see cref="EmailOptions.DevelopmentFile"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="emailAddress"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public async Task SendEmailAsync(string message, string emailAddress, string subject)
        {
            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_emailOptions.Name, _emailOptions.EmailAddress));
            mimeMessage.To.Add(new MailboxAddress(emailAddress, emailAddress));
            mimeMessage.Subject = subject;

            mimeMessage.Body = new TextPart("plain"){Text = message};

            mimeMessage.WriteTo(_emailOptions.DevelopmentFile);
        }
    }
}
