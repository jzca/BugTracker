using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace BugTracker.Models

{
    public class EmailService : IIdentityMessageService
    {
        //private string SmtpHost = "";
        //private int SmtpPort = 2525;
        //private string SmtpUsername = "";
        //private string SmtpPassword = "";
        //private string SmtpFrom = "";

        private string SmtpHost = ConfigurationManager.
            AppSettings["SmtpHost"];
        private int SmtpPort = Convert.ToInt32(ConfigurationManager.
            AppSettings["SmtpPort"]);
        private string SmtpUsername = ConfigurationManager.
            AppSettings["SmtpUsername"];
        private string SmtpPassword = ConfigurationManager.
            AppSettings["SmtpPassword"];
        private string SmtpFrom = ConfigurationManager.
            AppSettings["SmtpFrom"];

        public void Send(string to, string subject, string body)
        {
            var message = new MailMessage(SmtpFrom, to);
            message.Body = body;
            message.Subject = subject;
            message.IsBodyHtml = true;

            var smtpClient = new SmtpClient(SmtpHost, SmtpPort);
            smtpClient.Credentials = new NetworkCredential(SmtpUsername, SmtpPassword);
            smtpClient.EnableSsl = true;
            smtpClient.Send(message);

        }

        public Task SendAsync(IdentityMessage message)
        {
            // run async function for me
            return Task.Run(() =>
            Send(message.Destination, message.Body, message.Subject));
        }
    }
}