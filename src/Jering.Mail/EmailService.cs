using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace Jering.Mail
{
    /// <summary>
    /// Provides an API for handling emails.
    /// </summary>
    public class EmailService : IEmailService
    {
        protected EmailOptions _emailOptions { get; }
        protected SmtpClient _smtpClient { get; }

        /// <summary>
        /// Constructs an instance of <see cref="EmailService"/>. 
        /// </summary>
        /// <param name="emailOptionsAccessor"></param>
        /// <param name="smtpClient"></param>
        public EmailService(IOptions<EmailOptions> emailOptionsAccessor, SmtpClient smtpClient)
        {
            _emailOptions = emailOptionsAccessor.Value;
            _smtpClient = smtpClient;
        }

        /// <summary>
        /// Constructs a new <see cref="MimeMessage"/> instance from <paramref name="emailAddress"/>, <paramref name="subject"/> and <paramref name="message"/>.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <returns><see cref="MimeMessage"/> </returns>
        public virtual MimeMessage CreateMimeMessage(string emailAddress, string subject, string message)
        {
            if(emailAddress == null || subject == null || message == null)
            {
                throw new ArgumentNullException();
            }

            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_emailOptions.Name, _emailOptions.EmailAddress));
            mimeMessage.To.Add(new MailboxAddress(emailAddress, emailAddress));
            mimeMessage.Subject = subject;

            mimeMessage.Body = new TextPart("plain") { Text = message };

            return mimeMessage;
        }

        /// <summary>
        /// Sends an email with message <paramref name="mimeMessage"/>.
        /// </summary>
        /// <param name="mimeMessage"></param>
        /// <returns></returns>
        public virtual async Task SendEmailAsync(MimeMessage mimeMessage)
        {
            await _smtpClient.ConnectAsync(_emailOptions.Host, _emailOptions.Port, false);

            // Note: since we don't have an OAuth2 token, disable
            // the XOAUTH2 authentication mechanism.
            _smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");

            // Note: only needed if the SMTP server requires authentication
            await _smtpClient.AuthenticateAsync(_emailOptions.Username, _emailOptions.Password);

            await _smtpClient.SendAsync(mimeMessage);
            _smtpClient.Disconnect(true);
        }
    }
}
