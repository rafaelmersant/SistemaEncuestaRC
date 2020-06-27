using Sentry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace EncuestasRC.App_Start
{
    public static class Helper
    {
        public static void SendRawEmail(string emailto, string emailfrom, string firstname, string lastname, string subject, string body)
        {
            var mail = new MailMessage();
            var smtp = new SmtpClient();

            mail.From = new MailAddress(emailfrom, firstname + " " + lastname);

            if (emailto.Contains(";"))
            {
                var emails = emailto.Split(';');
                foreach (var email in emails)
                    mail.To.Add(email);
            }
            else
                mail.To.Add(emailto);

            mail.Subject = subject;

            mail.Body = body;
            mail.IsBodyHtml = true;

            try
            {
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.Write("Failed to send email for " + mail.To[0] + ", Error: " + ex.Message);
            }
        }
               
        public static void SendExceptionToSentry(Exception ex)
        {
            string _sentry = ConfigurationManager.AppSettings["sentry_dsn"];
            string _environment = ConfigurationManager.AppSettings["sentry_environment"];

            var ravenClient = new SharpRaven.RavenClient(_sentry);
            ravenClient.Environment = _environment;
            ravenClient.Capture(new SharpRaven.Data.SentryEvent(ex));
        }

        public static void SendExceptionToSentry(string message)
        {
            string _sentry = ConfigurationManager.AppSettings["sentry"];
            string _environment = ConfigurationManager.AppSettings["sentry_environment"];

            var ravenClient = new SharpRaven.RavenClient(_sentry);
            ravenClient.Environment = _environment;
            ravenClient.Capture(new SharpRaven.Data.SentryEvent(message));
        }
    }
}