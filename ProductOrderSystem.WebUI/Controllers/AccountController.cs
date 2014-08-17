using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.Entity;
using ProductOrderSystem.Domain.Models;
using ProductOrderSystem.WebUI.Abstract;
using ProductOrderSystem.WebUI.Helpers;
using ProductOrderSystem.WebUI.Models;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

namespace ProductOrderSystem.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private IAuthProvider authProvider;
        private IUserRepository userRepository;
        private static readonly OpenIdRelyingParty relyingParty = new OpenIdRelyingParty();
        private const string IDENTIFIER = "https://www.google.com/accounts/o8/id";

        public AccountController(IUserRepository userRepository, IAuthProvider auth)
        {
            this.userRepository = userRepository;
            this.authProvider = auth;
            HostMetaDiscoveryService googleAppsDiscovery = new HostMetaDiscoveryService
            {
                UseGoogleHostedHostMeta = true,
            };

            //relyingParty 
            relyingParty.DiscoveryServices.Insert(0, googleAppsDiscovery);
        }

        //
        // GET: /Account/

        public ActionResult LogOn()
        {
            IAuthenticationResponse authResponse = relyingParty.GetResponse();
            string returnUrl = Request["returnUrl"];
            if (authResponse != null)
            {
                switch (authResponse.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        FetchResponse fetch = authResponse.GetExtension<FetchResponse>();
                        if (fetch != null)
                        {
                            // Save user details in session variables
                            Session["email"] = fetch.GetAttributeValue(WellKnownAttributes.Contact.Email);
                            Session["firstName"] = fetch.GetAttributeValue(WellKnownAttributes.Name.First);
                            Session["lastName"] = fetch.GetAttributeValue(WellKnownAttributes.Name.Last);
                        }

                        // Set the authentication cookie for the user
                        string email = Session["email"].ToString();
                        if (authProvider.Authenticate(email, ""))
                        {
                            return Redirect(returnUrl ?? Url.Action("Index", "Home"));
                        }

                        else
                        {
                            ViewBag.ErrorMessage = string.Format("{0} have no access to this apps, please contact your team lead if your really need to access it.", email);
                            return View("Logoff");
                        }
                    //FormsAuthentication.SetAuthCookie(authResponse.ClaimedIdentifier, false);
                    //break;
                    case AuthenticationStatus.Canceled:
                        Response.Write("Login Canceled");
                        //loginCanceledLabel.Visible = true;
                        break;

                    case AuthenticationStatus.Failed:
                        Response.Write("Login Failed");
                        //loginFailedLabel.Visible = true;
                        break;
                }
            }

            else if (Request["domain"] != null)
            {
                string domain = Request["domain"];
                string x = string.Format("https://www.google.com/accounts/o8/site-xrds?hd={0}", domain);
                IAuthenticationRequest request = relyingParty.CreateRequest(Identifier.Parse(IDENTIFIER));
                SendGoogleRequest(request);
            }

            else
            {
                if (!Request.Url.Authority.Contains("localhost"))
                {
                    IAuthenticationRequest request = relyingParty.CreateRequest(Identifier.Parse(IDENTIFIER));
                    SendGoogleRequest(request);
                }
            }
            //User user = new User()
            //{
            //    Email = "guek1983@yahoo.com",
            //    LoginID = "guek1983@yahoo.com",
            //    Name = "Guek Tai Shan",
            //    Password = "123456",
            //    PhoneNo = "0127603347"
            //};

            //Role role = userRepository.Roles.Where(r => r.ID == 1).FirstOrDefault();

            //user.Roles.Add(role);

            //userRepository.SaveUser(user);
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //IAuthenticationRequest request = relyingParty.CreateRequest(model.UserName);
                //sendGoogleRequest(request);
                if (authProvider.Authenticate(model.UserName, ""))
                {
                    return Redirect(returnUrl ?? Url.Action("Index", "Home"));
                }

                else
                {
                    ModelState.AddModelError("", "You are not allow to access this apps.");
                    return View();
                }
            }

            else
            {
                return View();
            }
        }

        //
        // GET: /Account/LogOff
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session["FibreLogonUser"] = null;
            return RedirectToAction("Logout");
            //return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            return View("Logoff");
        }

        [Authorize]
        public ActionResult Profile(LogonUser logonUser)
        {
            User user = userRepository.GetUser(logonUser.Email);
            PopulateAssignedRoleData(user);
            //ViewBag.Roles = userRepository.Roles;// new SelectList(userRepository.Roles, "ID", "Name");
            ProfileModel userEdit = new ProfileModel()
            {
                PhoneNo = user.PhoneNo,
                Email = user.UserEmail,
                Name = user.Name,
                //Roles = user.Roles.Select(u => u.ID).ToArray()
            };

            return View(userEdit);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Profile(LogonUser logonUser, ProfileModel model)
        {
            User user = null;

            try
            {
                user = userRepository.Users.Where(u => u.UserEmail == logonUser.Email).FirstOrDefault();

                //model.Email = user.UserEmail;
                //model.State = user.State;
                if (ModelState.IsValid)
                {
                    model.Roles = user.Roles.Select(u => u.ID).ToArray();
                    user.PhoneNo = model.PhoneNo;
                    user.Name = model.Name;
                    userRepository.Update(user);
                    userRepository.Save();
                    TempData["message"] = "Profile has been successfully updated";

                    return RedirectToAction("Profile");
                }

                else
                {
                    PopulateAssignedRoleData(user);
                    ViewBag.ErrorMessage = "Profile failed to be updated";
                    return View(model);
                }
            }

            catch (Exception ex)
            {
                //ModelState.AddModelError("", c);
                PopulateAssignedRoleData(user);
                ViewBag.ErrorMessage = ex.ToString();
                return View(model);
            }
        }

        protected override void Dispose(bool disposing)
        {
            userRepository.Dispose();
            base.Dispose(disposing);
        }

        private void PopulateAssignedRoleData(User user = null)
        {
            DbSet<Role> allRoles = userRepository.Context.Roles;
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

            ViewBag.Roles = viewModel;
        }

        private void SendGoogleRequest(IAuthenticationRequest request)
        {
            // Request access to e-mail address, first name and last name
            // via OpenID Attribute Exchange (AX)
            FetchRequest fetch = new FetchRequest();
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.Email, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Name.First, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Name.Last, true));
            request.AddExtension(fetch);

            request.AddExtension(new ClaimsRequest { Email = DemandLevel.Require });

            // Send your visitor to their Provider for authentication.  
            request.RedirectToProvider();
        }
    }
}
