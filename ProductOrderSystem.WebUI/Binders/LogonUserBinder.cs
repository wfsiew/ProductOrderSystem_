using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductOrderSystem.Domain.Models;
using ProductOrderSystem.WebUI.Concrete;
using ProductOrderSystem.WebUI.Models;

namespace ProductOrderSystem.WebUI.Binders
{
    public class LogonUserBinder : IModelBinder
    {
        private const string SESSION_KEY = "FibreLogonUser";

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            LogonUser logonUser = (LogonUser)controllerContext.HttpContext.Session[SESSION_KEY];
            if (logonUser == null)
            {
                UserRepository userRepository = new UserRepository();
                User user = userRepository.Users.Where(u => u.UserEmail == controllerContext.HttpContext.User.Identity.Name).FirstOrDefault();
                List<Role> roles = user.Roles.ToList();
                logonUser = new LogonUser()
                {
                    UserID = user.ID,
                    Email = user.UserEmail,
                    LoginID = user.UserEmail,
                    Name = user.Name,
                    Roles = roles
                };
                controllerContext.HttpContext.Session[SESSION_KEY] = logonUser;
            }

            return logonUser;
        }
    }
}