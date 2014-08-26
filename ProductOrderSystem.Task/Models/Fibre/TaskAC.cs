using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.SqlServer;
using ProductOrderSystem.Domain.Models.Fibre;
using ProductOrderSystem.Task.Helpers;
using ProductOrderSystem.Task.Concrete;

namespace ProductOrderSystem.Task.Models.Fibre
{
    public class TaskAC
    {
        public TaskAC()
        {
            ToList = new List<string>();
        }

        public OrderRepository Repository { get; set; }

        public List<OrderFibre> Orders { get; set; }

        public List<string> ToList { get; set; }

        public string WebUrl { get; set; }

        public void GetTaskList()
        {
            var q = Repository.Orders.Where(x => x.ActionTypeID >= 5 && x.ActionTypeID <= 8 &&
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

                var acUsers = Repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.BILLING));
                ToList.AddRange(acUsers.Select(x => x.UserEmail).ToList());

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
                OrderFibre o = Orders[i];
                sb.Append("<li>");
                sb.AppendFormat(@"<a href=""{2}/{0}/{1}"" target=""_blank"">{1} {3}</a>", GetAction(o), o.ID, WebUrl, o.CustName);
                sb.Append("</li>");
            }

            sb.Append("</ol>");

            return sb.ToString();
        }

        private string GetAction(OrderFibre o)
        {
            string a = "DetailsAC";

            switch (o.ActionTypeID)
            {
                case 6:
                    a = "DetailsACResubmit";
                    break;

                case 7:
                    a = "DetailsACWithdraw";
                    break;

                case 8:
                    a = "DetailsACTerminate";
                    break;

                default:
                    a = "DetailsAC";
                    break;
            }

            return a;
        }
    }
}