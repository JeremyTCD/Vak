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
    /// Provides an API for handling emails.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private EmailOptions _emailOptions { get; }

        /// <summary>
        /// Constructs and instance of <see cref="EmailSender"/>. 
        /// </summary>
        /// <param name="emailOptionsAccessor"></param>
        public EmailSender(IOptions<EmailOptions> emailOptionsAccessor)
        {
            _emailOptions = emailOptionsAccessor.Value;
        }
        /// <summary>
        /// Writes an email with message <paramref name="message"/> and subject <paramref name="subject"/> to <paramref name="emailAddress"/>.
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

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailOptions.Host, _emailOptions.Port, false);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                // Note: only needed if the SMTP server requires authentication
                await client.AuthenticateAsync(_emailOptions.Username, _emailOptions.Password);

                await client.SendAsync(mimeMessage);
                client.Disconnect(true);
            }
        }
    }
}
