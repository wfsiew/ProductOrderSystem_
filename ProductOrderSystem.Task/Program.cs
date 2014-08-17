using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProductOrderSystem.Domain.Models;
using ProductOrderSystem.Task.Concrete;
using ProductOrderSystem.Task.Models;

namespace ProductOrderSystem.Task
{
    class Program
    {
        static string WebUrl = "http://login.redtone.com/ProductOrderSystem/Fibre/Order";

        static void Main(string[] args)
        {
            if (args.Length == 0)
                RunTask1();

            else
            {
                if (args[0] == "1")
                    RunTask1();

                else
                    RunTask2();
            }
        }

        static void RunTask1()
        {
            OrderRepository repository = null;

            try
            {
                repository = new OrderRepository();

                TaskCC taskcc = new TaskCC();
                taskcc.WebUrl = WebUrl;
                taskcc.Repository = repository;
                taskcc.GetTaskList();

                TaskInstall taskac = new TaskInstall();
                taskac.WebUrl = WebUrl;
                taskac.Repository = repository;
                taskac.GetTaskList();

                TaskFL taskfl = new TaskFL();
                taskfl.WebUrl = WebUrl;
                taskfl.Repository = repository;
                taskfl.GetTaskList();

                TaskInstall taskinstall = new TaskInstall();
                taskinstall.WebUrl = WebUrl;
                taskinstall.Repository = repository;
                taskinstall.GetTaskList();

                taskcc.Send();
                taskac.Send();
                taskfl.Send();
                taskinstall.Send();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            finally
            {
                if (repository != null)
                    repository.Dispose();
            }
        }

        static void RunTask2()
        {
            OrderRepository repository = null;

            try
            {
                repository = new OrderRepository();

                TaskCheckForm t = new TaskCheckForm();
                t.WebUrl = WebUrl;
                t.Repository = repository;
                t.GetTaskList();

                t.Send();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            finally
            {
                if (repository != null)
                    repository.Dispose();
            }
        }
    }
}