using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using GoogleAppHelper.Constants;
using GoogleAppHelper.Models;
using Google.GData.Client;
using Google.GData.Apps;

namespace GoogleAppHelper.Services
{
    public class GoogleUserRep
    {
        private string GoogleAppDomain;
        private string GoogleConsumerKey;
        private string GoogleConsumerSecret;
        private string GoogleApplicationName;
        private bool UseAdminLogin;
        private GOAuthRequestFactory requestFactory;

        public GoogleUserRep()
            : this(ConfigurationManager.AppSettings["GoogleApplicationName"],
             ConfigurationManager.AppSettings["GoogleAppDomain"],
             ConfigurationManager.AppSettings["GoogleConsumerKey"],
             ConfigurationManager.AppSettings["GoogleConsumerSecret"])
        {

        }

        public GoogleUserRep(string googleApplicationName, string googleAppDomain, string googleConsumerKey, string googleConsumerSecret)
        {
            GoogleAppDomain = googleAppDomain;
            GoogleConsumerKey = googleConsumerKey;
            GoogleConsumerSecret = googleConsumerSecret;
            GoogleApplicationName = googleApplicationName;
            UseAdminLogin = Convert.ToBoolean(ConfigurationManager.AppSettings["GoogleUseAdminLogin"]);
            requestFactory = new GOAuthRequestFactory("cl", GoogleApplicationName);
            requestFactory.ConsumerKey = ConfigurationManager.AppSettings["GoogleConsumerKey"];
            requestFactory.ConsumerSecret = ConfigurationManager.AppSettings["GoogleConsumerSecret"];
        }


        //public List<GoogleUser> GetAllDomainUsers(string adminLoginId, string adminPassword)
        //{
        //    List<GoogleUser> googleUsers = new List<GoogleUser>();


        //    try
        //    {
        //        AppsService service = new AppsService(GoogleAppDomain, adminLoginId, adminPassword);

        //        UserFeed resultfeed = service.RetrieveAllUsers();

        //        if (resultfeed.Entries.Count > 0)
        //        {
        //            foreach (UserEntry entry in resultfeed.Entries)
        //            {
        //                googleUsers.Add(new GoogleUser()
        //                {
        //                    Domain = GoogleAppDomain,
        //                    Name = entry.Name.GivenName + ' ' + entry.Name.FamilyName,
        //                    UserName = entry.Login.UserName
        //                });
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //    return googleUsers;
        //}


        public List<GoogleUser> GetAllDomainUsers()
        {
            List<GoogleUser> googleUsers = new List<GoogleUser>();

            try
            {
                requestFactory.ConsumerKey = GoogleConsumerKey;
                requestFactory.ConsumerSecret = GoogleConsumerSecret;

                UserService service = new UserService(GoogleApplicationName);

                if (UseAdminLogin)
                {
                    service.setUserCredentials(Constants.Constants.ADMIN_EMAIL, Constants.Constants.ADMIN_PWD);
                }

                else
                {
                    service.RequestFactory = requestFactory;
                }

                // Query the service with the parameters passed to the function
                UserQuery query = new UserQuery(GoogleAppDomain);
                query.Uri = new Uri("https://apps-apis.google.com/a/feeds/user/");
                //query.OAuthRequestorId = requestorId;
                query.NumberToRetrieve = 200;

                UserFeed resultfeed = service.Query(query);

                if (resultfeed.Entries.Count > 0)
                {
                    foreach (UserEntry entry in resultfeed.Entries)
                    {
                        googleUsers.Add(new GoogleUser()
                        {
                            Domain = GoogleAppDomain,
                            Name = entry.Name.GivenName,
                            UserName = entry.Login.UserName
                        });
                    }
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }

            return googleUsers;
        }
    }
}
