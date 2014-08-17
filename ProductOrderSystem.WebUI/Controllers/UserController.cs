using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ProductOrderSystem.Domain.Models;
using GoogleAppHelper.Models;
using GoogleAppHelper.Services;
using ProductOrderSystem.WebUI.Abstract;
using ProductOrderSystem.WebUI.Concrete;
using ProductOrderSystem.WebUI.Helpers;
using ProductOrderSystem.WebUI.Models;

namespace ProductOrderSystem.WebUI.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private IUserRepository repository;

        public UserController(IUserRepository repository)
        {
            this.repository = repository;
        }

        public ActionResult Index()
        {
            ViewBag.Menu = Constants.USER_MANAGEMENT;
            ViewBag.IsSuperAdmin = User.IsInRole(Constants.SUPER_ADMIN);

            return View();
        }

        public ActionResult Search(string sortOrder, string searchString, int? page)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            if (!User.IsInRole(Constants.SUPER_ADMIN))
            {
                res["error"] = 1;
                res["message"] = "You are not authorize to view this site";
                return Json(res, JsonRequestBehavior.AllowGet);
            }

            string keyword = string.IsNullOrEmpty(searchString) ? null : searchString.ToUpper();

            if (searchString != null)
                page = 1;

            var q = repository.Users.Where(x => x.Status == 1);

            if (!string.IsNullOrEmpty(keyword))
            {
                q = q.Where(x => x.Name.ToUpper().Contains(keyword) ||
                    x.UserEmail.ToUpper().Contains(keyword) || x.PhoneNo.ToUpper().Contains(keyword));
            }

            switch (sortOrder)
            {
                case "Name_desc":
                    q = q.OrderByDescending(x => x.Name);
                    break;

                case "Email":
                    q = q.OrderBy(x => x.UserEmail);
                    break;

                case "Email_desc":
                    q = q.OrderByDescending(x => x.UserEmail);
                    break;

                case "PhoneNo":
                    q = q.OrderBy(x => x.PhoneNo);
                    break;

                case "PhoneNo_desc":
                    q = q.OrderByDescending(x => x.PhoneNo);
                    break;

                default:
                    q = q.OrderBy(x => x.Name);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int total = q.Count();

            Pager pager = new Pager(total, pageNumber, pageSize);
            var l = q.Skip(pager.LowerBound).Take(pager.PageSize).ToList();
            var lx = l.Select(x => new
            {
                ID = x.ID,
                Name = x.Name,
                UserEmail = x.UserEmail,
                PhoneNo = x.PhoneNo,
                Roles = string.Join(",", x.ArrRoles)
            });

            res["pager"] = pager;
            res["model"] = lx;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(LogonUser logonUser, UserCreateModel model, string[] selectedRoles)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.SUPER_ADMIN))
                {
                    res["error"] = 1;
                    res["message"] = "You are not authorize to view this site";
                }

                if (selectedRoles == null)
                {
                    res["error"] = 1;
                    res["message"] = "Please select at least 1 role";
                }

                if (ModelState.IsValid && res.Count < 1)
                {
                    GoogleUser googleUser = null;

                    if (model.Email == Constants.STAFF_EMAIL)
                    {
                        googleUser = new GoogleUser { Name = Constants.STAFF_APPLICATION };
                    }

                    else
                    {
                        List<GoogleUser> unregisteredUsers = GetUnregisterGoogleUser();
                        googleUser = unregisteredUsers.Where(x => x.UserEmail == model.Email).FirstOrDefault();
                    }

                    //todo: local test
                    //googleUser = new GoogleUser { Name = "ken" };

                    User user = new User
                    {
                        Name = googleUser.Name,
                        PhoneNo = model.PhoneNo,
                        Status = 1,
                        UserEmail = model.Email
                    };

                    repository.UpdateUserRoles(selectedRoles, user);
                    repository.Insert(user);
                    repository.Save();
                    res["success"] = 1;
                    res["message"] = string.Format("User {0} <{1}> has been successfully created", user.Name, user.UserEmail);
                }
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.SUPER_ADMIN))
                {
                    res["error"] = 1;
                    res["message"] = "You are not authorize to view this site";
                }

                if (res.Count < 1)
                {
                    User user = repository.Users.Where(x => x.ID == id).FirstOrDefault();
                    if (user == null)
                    {
                        res["error"] = 1;
                        res["message"] = "User not found";
                    }

                    else
                    {
                        var o = new
                        {
                            Email = user.UserEmail,
                            ID = user.ID,
                            Name = user.Name,
                            PhoneNo = user.PhoneNo,
                            Roles = GetRoles(user),
                            Emails = GetUnregisteredGoogleUsers()
                        };
                        return Json(o, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit(UserEditModel model, string[] selectedRoles)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.SUPER_ADMIN))
                {
                    res["error"] = 1;
                    res["message"] = "You are not authorize to view this site";
                }

                if (selectedRoles == null)
                {
                    res["error"] = 1;
                    res["message"] = "Please select at least 1 role";
                }

                if (ModelState.IsValid && res.Count < 1)
                {
                    User user = repository.Users.Where(x => x.ID == model.ID).FirstOrDefault();
                    if (user == null)
                    {
                        res["error"] = 1;
                        res["message"] = "User not found";
                    }

                    else
                    {
                        user.Name = model.Name;
                        user.PhoneNo = model.PhoneNo;
                        repository.UpdateUserRoles(selectedRoles, user);
                        repository.Update(user);
                        repository.Save();
                        res["success"] = 1;
                        res["message"] = string.Format("User {0} <{1}> has been successfully updated", user.Name, user.UserEmail);
                    }
                }
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(LogonUser logonUser, int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.SUPER_ADMIN))
                {
                    res["error"] = 1;
                    res["message"] = "You are not authorize to view this site";
                }

                if (logonUser.UserID == id)
                {
                    res["error"] = 1;
                    res["message"] = "You cannot delete your own account";
                }

                if (res.Count < 1)
                {
                    User user = repository.Users.Where(x => x.ID == id).FirstOrDefault();
                    if (user == null)
                    {
                        res["error"] = 1;
                        res["message"] = "User not found";
                    }

                    else
                    {
                        repository.Delete(user);
                        repository.Save();
                        res["success"] = 1;
                        res["message"] = string.Format("User {0} <{1}> has been successfully deleted", user.Name, user.UserEmail);
                    }
                }
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Roles()
        {
            List<AssignedRoleData> o = GetRoles();
            return Json(o, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UnregisteredGoogleUsers()
        {
            var o = GetUnregisteredGoogleUsers();
            return Json(o, JsonRequestBehavior.AllowGet);
        }

        public List<GoogleUser> GetUnregisterGoogleUser()
        {
            try
            {
                GoogleUserRep gUserRep = new GoogleUserRep();
                var users = from u in repository.Users
                            where u.Status == 1
                            select u.UserEmail;

                var googleUsers = from guser in gUserRep.GetAllDomainUsers()
                                  where !users.Contains(guser.UserEmail)
                                  select guser;

                return googleUsers.ToList();
            }

            catch
            {
                return new List<GoogleUser>();
            }
        }

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }

        private void PopulateAssignedRoleData(UserCreateModel user = null)
        {
            DbSet<Role> allRoles = repository.Context.Roles;
            List<int> userRoles = null;

            if (user != null)
                userRoles = new List<int>(user.Roles);

            List<AssignedRoleData> viewModel = new List<AssignedRoleData>();
            foreach (Role role in allRoles)
            {
                viewModel.Add(new AssignedRoleData
                {
                    RoleID = role.ID,
                    Name = role.Name,
                    Assigned = user == null ? false : userRoles.Contains(role.ID)
                });
            }

            ViewBag.Roles = viewModel;
        }

        private void PopulateUnregisteredUsersDropDownList(object selectedEmail = null)
        {
            List<GoogleUser> unRegisteredUsers = GetUnregisterGoogleUser();
            ViewBag.UnregisterUsers_ = new SelectList(unRegisteredUsers, "UserEmail", "UserEmail", selectedEmail);
        }

        private List<AssignedRoleData> GetRoles(User user = null)
        {
            DbSet<Role> allRoles = repository.Context.Roles;
            HashSet<int> userRoles = null;

            if (user != null)
                userRoles = new HashSet<int>(user.Roles.Select(x => x.ID));

            List<AssignedRoleData> viewModel = new List<AssignedRoleData>();
            foreach (Role role in allRoles)
            {
                viewModel.Add(new AssignedRoleData
                {
                    RoleID = role.ID,
                    Name = role.Name,
                    Assigned = user == null ? false : userRoles.Contains(role.ID)
                });
            }

            return viewModel;
        }

        private object GetUnregisteredGoogleUsers()
        {
            List<GoogleUser> l = GetUnregisterGoogleUser();
            var o = l.Select(x => x.UserEmail);
            return o;
        }
    }
}
