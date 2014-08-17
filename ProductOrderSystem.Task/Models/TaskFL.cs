using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.SqlServer;
using ProductOrderSystem.Domain.Fibre.Models;
using ProductOrderSystem.Task.Helpers;
using ProductOrderSystem.Task.Concrete;

namespace ProductOrderSystem.Task.Models
{
    public class TaskFL
    {
        public TaskFL()
        {
            ToList = new List<string>();
        }

        public OrderRepository Repository { get; set; }

        public List<Order> Orders { get; set; }

        public List<string> ToList { get; set; }

        public string WebUrl { get; set; }

        public void GetTaskList()
        {
            var q = Repository.Orders.Where(x => x.ActionTypeID >= 9 && x.ActionTypeID <= 12 &&
                SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 1);
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

                var flUsers = Repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.FIXED_LINE));
                ToList.AddRange(flUsers.Select(x => x.UserEmail).ToList());

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
                Order o = Orders[i];
                sb.Append("<li>");
                sb.AppendFormat(@"<a href=""{2}/{0}/{1}"" target=""_blank"">{1} {3}</a>", GetAction(o), o.ID, WebUrl, o.CustName);
                sb.Append("</li>");
            }

            sb.Append("</ol>");

            return sb.ToString();
        }

        private string GetAction(Order o)
        {
            string a = "DetailsFL";

            switch (o.ActionTypeID)
            {
                case 10:
                    a = "DetailsFLResubmit";
                    break;

                case 11:
                    a = "DetailsFLWithdraw";
                    break;

                case 12:
                    a = "DetailsFLTerminate";
                    break;

                default:
                    a = "DetailsFL";
                    break;
            }

            return a;
        }
    }
}