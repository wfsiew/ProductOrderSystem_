﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.SqlServer;
using ProductOrderSystem.Domain.Models.Fibre;
using ProductOrderSystem.Task.Helpers;
using ProductOrderSystem.Task.Concrete;

namespace ProductOrderSystem.Task.Models.Fibre
{
    public class TaskCheckForm
    {
        public TaskCheckForm()
        {
            ToList = new List<string>();
        }

        public OrderRepository Repository { get; set; }

        public List<OrderFibre> Orders { get; set; }

        public List<string> ToList { get; set; }

        public string WebUrl { get; set; }

        public void GetTaskList()
        {
            var q = Repository.Orders.Where(x => x.IsFormReceived == false && 
                x.StatusSC != 2 &&
                SqlFunctions.DateDiff("day", x.CreateDatetime, DateTime.Now) > 3);
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

                var scUsers = Repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));
                ToList.AddRange(scUsers.Select(x => x.UserEmail).ToList());

                //todo: test send updated order email
                //ToList = new List<string>();

                if (ToList != null)
                {
                    ToList.Add("siewwingfei@hotmail.com");
                    //ToList.Add("siewwah.tham@redtone.com");
                }

                MailCheckFormSender sender = new MailCheckFormSender();
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
                sb.AppendFormat(@"<a href=""{2}/{0}/{1}"" target=""_blank"">{1} {3}</a>", "Details", o.ID, WebUrl, o.CustName);
                sb.Append("</li>");
            }

            sb.Append("</ol>");

            return sb.ToString();
        }
    }
}