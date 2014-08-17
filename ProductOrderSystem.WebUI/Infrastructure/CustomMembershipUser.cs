using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace ProductOrderSystem.WebUI.Infrastructure
{
    public class CustomMembershipUser : MembershipUser
    {
        public int UserID { get; set; }
    }
}