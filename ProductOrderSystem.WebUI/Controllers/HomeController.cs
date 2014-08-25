using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductOrderSystem.Domain.Models;
using ProductOrderSystem.WebUI.Abstract.Fibre;
using ProductOrderSystem.WebUI.Concrete.Fibre;
using ProductOrderSystem.WebUI.Helpers;
using ProductOrderSystem.WebUI.Models;

namespace ProductOrderSystem.WebUI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private IOrderRepository repository;

        public HomeController(IOrderRepository repository)
        {
            this.repository = repository;
        }

        //
        // GET: /Home/

        public ActionResult Index()
        {
            ViewBag.Menu = Constants.HOME;
            ViewBag.Role = Utils.Role(User);

            return View();
        }
    }
}
