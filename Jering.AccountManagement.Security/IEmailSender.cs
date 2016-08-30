using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.AccountManagement.Security
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="emailAddress"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        Task SendEmailAsync(string message, string emailAddress, string subject);
    }
}
