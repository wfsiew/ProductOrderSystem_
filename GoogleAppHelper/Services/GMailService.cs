using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using GoogleAppHelper.Models;
using Google.GData.Client;
using Google.GData.Apps;

namespace GoogleAppHelper.Services
{
    public class GMailService
    {
        private int Port { get; set; }
        private string AccountName { get; set; }
        private string Password { get; set; }

        private const string SMTP_SERVER = "mail.redtone.com";

        public GMailService()
            : this("", "")
        {
        }

        public GMailService(string accountName, string password)
        {
            Port = 587;
            AccountName = accountName;
            Password = password;
        }

        public void Send(string from, string fromDisplayName, string to, string subject, string body, bool isHtml)
        {
            Send(from, fromDisplayName, to, subject, body, isHtml, null);
        }

        public void Send(string from, string fromDisplayName, string to, string subject, string body, bool isHtml, string[] attachments)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient(SMTP_SERVER, Port);
                smtpClient.EnableSsl = false;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                //smtpClient.Credentials = new NetworkCredential(AccountName, Password);

                MailMessage message = new MailMessage();

                string[] tos = to.Split(';');

                foreach (string toEmail in tos)
                {
                    message.To.Add(toEmail);
                }

                message.Subject = subject;
                message.From = new MailAddress(from, fromDisplayName);
                message.Body = body;
                message.IsBodyHtml = isHtml;
                if (attachments != null)
                {
                    for (int i = 0; i < attachments.Length; i++)
                    {
                        message.Attachments.Add(new Attachment(attachments[i]));
                    }
                }

                smtpClient.Send(message);
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }


        //public void Send(string from, string to, string subject, string body, bool isHtml, string[] attachments)
        //{
        //    try
        //    {
        //        SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", _port);
        //        smtpClient.EnableSsl = true;
        //        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        //        smtpClient.UseDefaultCredentials = false;
        //        smtpClient.Credentials = new NetworkCredential(_accountName, _password);

        //        MailMessage message = new MailMessage();
        //        message.To.Add(new MailAddress(to));
        //        message.Subject = subject;
        //        message.From = new MailAddress(from);
        //        message.Body = body;
        //        message.IsBodyHtml = isHtml;
        //        if (attachments != null)
        //        {
        //            for (int i = 0; i < attachments.Length; i++)
        //            {
        //                message.Attachments.Add(new Attachment(attachments[i]));
        //            }
        //        }
        //        smtpClient.Send(message);

        //    }
        //    catch (Exception ex)
        //    {
        //        return;
        //    }
        //}

        public static void SendMail(string senderAddress, string recepientAddress, string subject, string body,
                                        string host, int port, string username, string password, bool enabledSSL)
        {
            try
            {
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(senderAddress);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;

                    mailMessage.IsBodyHtml = true;
                    mailMessage.To.Add(new MailAddress(recepientAddress));

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = host;
                    smtp.EnableSsl = enabledSSL;
                    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential(username, password);
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = port;
                    smtp.Send(mailMessage);
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
