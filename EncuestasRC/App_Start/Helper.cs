using EncuestasRC.Models;
using Sentry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
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
                EnableSsl = false,
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

        public static void UpdateSortIndexQuestions(int surveyId, int questionId, int sortIndex)
        {
            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    bool sort = false;
                    int index = 1;

                    var questions = db.Questions.Where(q => q.SortIndex >= sortIndex && q.Id != questionId && q.SurveyId == surveyId).OrderBy(o => o.SortIndex).ToList();

                    foreach (var question in questions)
                    {
                        if (question.SortIndex == sortIndex)
                            sort = true;

                        if (sort)
                        {
                            if (questions.Count() == 1)
                                question.SortIndex = sortIndex - 1;
                            else
                                question.SortIndex = sortIndex + index;

                            db.SaveChanges();
                        }

                        index += 1;
                    }

                    var allQuestions = db.Questions.Where(q => q.SurveyId == surveyId).OrderBy(o => o.SortIndex).ToList();
                    index = 1;

                    foreach (var question in allQuestions)
                    {
                        question.SortIndex = index;
                        db.SaveChanges();

                        index += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }
        }

        public static DataSet GetCustomerName(string order)
        {
            string orderNo = order;
            string orderType = string.Empty;
            string sQuery = string.Empty;
            string environmentID = ConfigurationManager.AppSettings["EnvironmentEncuestas"];

            if (order.Contains("-"))
            {
                orderNo = order.Split('-')[0];
                orderType = order.Split('-')[1];
            }
                
            if (string.IsNullOrEmpty(orderType))
                sQuery = "SELECT OSNOMC as nombreCte, OSTIFT as TipoFactura, OSFACO as NoFactura, OSTIDO OrdenTipo FROM [QS36F.RCOSMF00] WHERE OSNUOS = " + orderNo;
            else 
                sQuery = "SELECT OSNOMC as nombreCte, OSTIFT as TipoFactura, OSFACO as NoFactura, OSTIDO OrdenTipo FROM [QS36F.RCOSMF00] WHERE OSTIDO IN ('" + orderType + "') AND OSNUOS = " + orderNo;

            if (environmentID != "DEV")
                sQuery = sQuery.Replace("[", "").Replace("]", "");

            return ExecuteDataSetODBC(sQuery, null);
        }

        public static DataSet ExecuteDataSetODBC(string query, OdbcParameter[] parameters = null)
        {
            string sConn = ConfigurationManager.AppSettings["sConnSQLODBC"];

            OdbcConnection oconn = new OdbcConnection(sConn);
            OdbcCommand ocmd = new OdbcCommand(query, oconn);
            OdbcDataAdapter adapter;
            DataSet dsData = new DataSet();

            ocmd.CommandType = CommandType.Text;

            if (parameters != null)
            {
                ocmd.Parameters.Clear();

                foreach (OdbcParameter param in parameters)
                    ocmd.Parameters.Add(param);
            }

            adapter = new OdbcDataAdapter(ocmd);
            adapter.Fill(dsData);

            return dsData;
        }
    }
}