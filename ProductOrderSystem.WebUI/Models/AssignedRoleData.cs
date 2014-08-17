using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductOrderSystem.WebUI.Models
{
    public class AssignedRoleData
    {
        public int RoleID { get; set; }
        public string Name { get; set; }
        public bool Assigned { get; set; }
    }
}