using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductOrderSystem.WebUI.Models;

namespace PorductOrderSystem.WebUI.Controllers
{
    public class OrderController : Controller
    {
        //
        // GET: /Order/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create(LogonUser logonUser, int? id, int? ordertypeid)
        {
            return RedirectToAction("Create", "Order", new
            {
                area = "Fibre",
                id = id,
                ordertypeid = ordertypeid
            });
        }

        public ActionResult Search(LogonUser logonUser)
        {
            return RedirectToAction("Search", "Order", new
            {
                area = "Fibre"
            });
        }

        public ActionResult Details(int id)
        {
            return RedirectToAction("Details", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsCC(int id)
        {
            return RedirectToAction("DetailsCC", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsCCResubmit(int id)
        {
            return RedirectToAction("DetailsCCResubmit", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsCCWithdraw(int id)
        {
            return RedirectToAction("DetailsCCWithdraw", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsCCTerminate(int id)
        {
            return RedirectToAction("DetailsCCTerminate", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsFL(int id)
        {
            return RedirectToAction("DetailsFL", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsFLResubmit(int id)
        {
            return RedirectToAction("DetailsFLResubmit", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsFLWithdraw(int id)
        {
            return RedirectToAction("DetailsFLWithdraw", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsFLTerminate(int id)
        {
            return RedirectToAction("DetailsFLTerminate", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsAC(int id)
        {
            return RedirectToAction("DetailsAC", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsACResubmit(int id)
        {
            return RedirectToAction("DetailsACResubmit", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsACWithdraw(int id)
        {
            return RedirectToAction("DetailsACWithdraw", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsACTerminate(int id)
        {
            return RedirectToAction("DetailsACTerminate", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsInstall(int id)
        {
            return RedirectToAction("DetailsInstall", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsInstallResubmit(int id)
        {
            return RedirectToAction("DetailsInstallResubmit", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsInstallResubmit1(int id)
        {
            return RedirectToAction("DetailsInstallResubmit1", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsInstallWithdraw(int id)
        {
            return RedirectToAction("DetailsInstallWithdraw", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }

        public ActionResult DetailsInstallTerminate(int id)
        {
            return RedirectToAction("DetailsInstallTerminate", "Order", new
            {
                area = "Fibre",
                id = id
            });
        }
    }
}
