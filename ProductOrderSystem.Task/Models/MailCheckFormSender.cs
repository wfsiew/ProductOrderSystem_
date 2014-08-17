using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Mail;
using ProductOrderSystem.Task.Helpers;

namespace ProductOrderSystem.Task.Models
{
    public class MailCheckFormSender : MailSender
    {
        public MailCheckFormSender()
        {
            Message = new MailMessage();
            Message.Subject = "ProductOrderSystem - Order Form Reminder";
            Message.From = new MailAddress(Constants.MAIL_SENDER, "ProductOrderSystem - Order Form Reminder");
            Message.IsBodyHtml = true;
        }

        protected override string LoadTemplate()
        {
            string dir = Directory.GetCurrentDirectory();
            string file = Path.Combine(dir, "Template/MailCheckForm.html");
            string a = null;

            using (StreamReader sr = new StreamReader(file))
            {
                a = sr.ReadToEnd();
            }

            return a;
        }
    }
}