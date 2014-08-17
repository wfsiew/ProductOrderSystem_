using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Mail;
using ProductOrderSystem.Task.Helpers;

namespace ProductOrderSystem.Task.Models
{
    public class MailSender
    {
        protected MailMessage Message { get; set; }
        public List<string> ToList { get; set; }
        public string Content { get; set; }

        public MailSender()
        {
            Message = new MailMessage();
            Message.Subject = "ProductOrderSystem - Order Task Reminder";
            Message.From = new MailAddress(Constants.MAIL_SENDER, "ProductOrderSystem - Task Reminder");
            Message.IsBodyHtml = true;
        }

        public void Send()
        {
            string body = LoadTemplate();
            body = body.Replace("{{content}}", Content);

            Message.Body = body;
            Message.BodyEncoding = Encoding.UTF8;

            for (int i = 0; i < ToList.Count; i++)
            {
                Message.To.Add(ToList[i]);
            }

            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Host = "mail.redtone.com";
                smtp.Port = 587;

                //todo: local test
                //smtp.PickupDirectoryLocation = "c:\\mails";

                smtp.Send(Message);
            }
        }

        protected virtual string LoadTemplate()
        {
            string dir = Directory.GetCurrentDirectory();
            string file = Path.Combine(dir, "Template/Mail.html");
            string a = null;

            using (StreamReader sr = new StreamReader(file))
            {
                a = sr.ReadToEnd();
            }

            return a;
        }
    }
}
