using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductOrderSystem.Domain.Models;
using ProductOrderSystem.WebUI.Abstract;
using ProductOrderSystem.WebUI.Concrete;
using ProductOrderSystem.WebUI.Models;

namespace ProductOrderSystem.WebUI.Controllers
{
    public class PersonController : Controller
    {
        private IUserRepository repository;

        public PersonController(IUserRepository repository)
        {
            this.repository = repository;
        }

        //
        // GET: /Person/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(UserCreateModel person)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                person = new UserCreateModel();
                person.Email = "wingfei.siew@redtone.com";
                person.Name = "wfsiew";
                person.PhoneNo = "777";
                string[] selectedRoles = new string[] { "1" };

                User user = new User
                {
                    Name = person.Name,
                    PhoneNo = person.PhoneNo,
                    Status = 1,
                    UserEmail = person.Email
                };

                repository.UpdateUserRoles(selectedRoles, user);
                repository.Insert(user);
                repository.Save();

                res["success"] = 1;
                res["message"] = string.Format("{0} has been saved", person.Name);
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }
    }
}
