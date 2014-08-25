using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.SqlServer;
using ProductOrderSystem.Domain.Fibre.Models;
using ProductOrderSystem.Task.Helpers;
using ProductOrderSystem.Task.Concrete;

namespace ProductOrderSystem.Task.Models.Fibre
{
    public class TaskInstall
    {
        public TaskInstall()
        {
            ToList = new List<string>();
        }

        public OrderRepository Repository { get; set; }

        public List<Order_Fibre> Orders { get; set; }

        public List<string> ToList { get; set; }

        public string WebUrl { get; set; }

        public void GetTaskList()
        {
            var q = Repository.Orders.Where(x => (x.ActionTypeID == 13 &&
                SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 3) ||
                (x.ActionTypeID == 14 &&
                SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 1) ||
                (x.ActionTypeID == 15 &&
                SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 3) ||
                (x.ActionTypeID == 16 &&
                SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 1) ||
                (x.ActionTypeID == 17 &&
                SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 3));
            Orders = q.ToList();
        }

        public void Send()
        {
            try
            {
                if (Orders == null)
                    return;

                if (Orders.Count < 1)
                    return;

                var installUsers = Repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.INSTALLERS));
                ToList.AddRange(installUsers.Select(x => x.UserEmail).ToList());

                //todo: test send updated order email
                //ToList = new List<string>();

                if (ToList != null)
                {
                    ToList.Add("siewwingfei@hotmail.com");
                    //ToList.Add("siewwah.tham@redtone.com");
                }

                MailSender sender = new MailSender();
                sender.ToList = ToList;
                sender.Content = GetMailContent();
                sender.Send();
            }
            
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private string GetMailContent()
        {
            StringBuilder sb = new StringBuilder("<ol>");

            for (int i = 0; i < Orders.Count; i++)
            {
                Order_Fibre o = Orders[i];
                sb.Append("<li>");
                sb.AppendFormat(@"<a href=""{2}/{0}/{1}"" target=""_blank"">{1} {3}</a>", GetAction(o), o.ID, WebUrl, o.CustName);
                sb.Append("</li>");
            }

            sb.Append("</ol>");

            return sb.ToString();
        }

        private string GetAction(Order_Fibre o)
        {
            string a = "DetailsInstall";

            switch (o.ActionTypeID)
            {
                case 14:
                    a = "DetailsInstallResubmit";
                    break;

                case 15:
                    a = "DetailsInstallResubmit1";
                    break;

                case 16:
                    a = "DetailsInstallWithdraw";
                    break;

                case 17:
                    a = "DetailsInstallTerminate";
                    break;

                default:
                    a = "DetailsInstall";
                    break;
            }

            return a;
        }
    }
}