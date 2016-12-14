using MimeKit;
using System.Threading.Tasks;

namespace Jering.Mail
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mimeMessage"></param>
        /// <returns></returns>
        Task SendEmailAsync(MimeMessage mimeMessage);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="emailAddress"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        MimeMessage CreateMimeMessage(string emailAddress, string subject, string message);
    }
}
