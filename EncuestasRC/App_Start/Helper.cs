using Sentry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
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
                Helper.SendException(ex);
            }
        }
               
        public static void SendException(Exception ex, string extraInfo = "")
        {
            string _sentry = ConfigurationManager.AppSettings["sentry_dsn"];
            string _environment = ConfigurationManager.AppSettings["sentry_environment"];

            var ravenClient = new SharpRaven.RavenClient(_sentry);
            ravenClient.Environment = _environment;

            var exception = new SharpRaven.Data.SentryEvent(ex);

            if (!string.IsNullOrEmpty(extraInfo))
                exception.Extra = extraInfo;

            ravenClient.Capture(exception);
        }

        public static void SendException(string message)
        {
            string _sentry = ConfigurationManager.AppSettings["sentry"];
            string _environment = ConfigurationManager.AppSettings["sentry_environment"];

            var ravenClient = new SharpRaven.RavenClient(_sentry);
            ravenClient.Environment = _environment;
            ravenClient.Capture(new SharpRaven.Data.SentryEvent(message));
        }

        public static string SHA256(string randomString)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));

            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("X2"));
            }
            return hash.ToString();
        }

        public static bool SendRecoverPasswordEmail(string newPassword, string email)
        {
            string content = "Su nueva contraseña es: <b>" + newPassword + "</b>";

            SmtpClient smtp = new SmtpClient
            {
                Host = ConfigurationManager.AppSettings["smtpClient"],
                Port = int.Parse(ConfigurationManager.AppSettings["PortMail"]),
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(ConfigurationManager.AppSettings["usrEmail"], ConfigurationManager.AppSettings["pwdEmail"]),
                EnableSsl = true,
            };

            MailMessage message = new MailMessage();
            message.IsBodyHtml = true;
            message.Body = content;
            message.Subject = "NUEVA CONTRASEÑA SISTEMA DE ENCUESTAS";
            message.To.Add(new MailAddress(email));

            string address = ConfigurationManager.AppSettings["EMail"];
            string displayName = ConfigurationManager.AppSettings["EMailName"];
            message.From = new MailAddress(address, displayName);

            try
            {
                smtp.Send(message);

                return true;
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);

                return false;
            }
        }
    }
}