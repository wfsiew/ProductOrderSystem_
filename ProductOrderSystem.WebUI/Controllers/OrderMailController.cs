using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ActionMailer.Net.Mvc;
using ProductOrderSystem.Domain.Fibre.Models;
using ProductOrderSystem.WebUI.Models;
using ProductOrderSystem.WebUI.Areas.Fibre.Models;
using ProductOrderSystem.WebUI.Helpers;

namespace ProductOrderSystem.WebUI.Controllers
{
    public class OrderMailController : MailerBase
    {
        public EmailResult OrderNotificationEmail(Order_Fibre o, EmailInfo mail, ViewDataDictionary viewData, bool updated = false)
        {
            foreach (string email in mail.ToList)
            {
                To.Add(email);
            }

            From = string.Format("{0} {1}", mail.DisplayName, Constants.MAIL_SENDER);
            Subject = mail.Subject;

            string view = updated ? "OrderUpdatedNotification" : "OrderCreatedNotification";
            ViewData = viewData;

            return Email(view, o);
        }

        public EmailResult OrderInstallDateChangedEmail(Order_Fibre o, EmailInfo mail, ViewDataDictionary viewData)
        {
            foreach (string email in mail.ToList)
            {
                To.Add(email);
            }

            From = string.Format("{0} {1}", mail.DisplayName, Constants.MAIL_SENDER);
            Subject = mail.Subject;

            string view = "OrderInstallDateNotification";
            ViewData = viewData;

            return Email(view, o);
        }
    }
}