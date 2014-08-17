using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ProductOrderSystem.Domain.Models;

namespace ProductOrderSystem.WebUI.Models
{
    public class LogOnModel
    {
        [Required]
        [Display(Name = "Email")]
        public string UserName { get; set; }
    }

    public class ProfileModel
    {
        [Display(Name = "Email")]
        [RegularExpression("[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Display(Name = "Contact No.")]
        public string PhoneNo { get; set; }

        [Display(Name = "Manager")]
        public int? ManagerID { get; set; }

        [Display(Name = "Role")]
        public int[] Roles { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
    }

    public class UserBaseModel
    {
        [Required]
        [Display(Name = "Email")]
        [RegularExpression("[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Display(Name = "Contact No.")]
        public string PhoneNo { get; set; }

        [Required]
        [Display(Name = "Role")]
        public int[] Roles { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
    }

    public class UserEditModel : UserBaseModel
    {
        [Required]
        public int ID { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class UserCreateModel : UserBaseModel
    {
        public UserCreateModel()
            : base()
        {
            Email = "";
            PhoneNo = "";
            ReturnUrl = "";
            Roles = new int[0];
        }

        public string ReturnUrl { get; set; }
    }

    public class LogonUser
    {
        public int UserID { get; set; }
        public string LoginID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<Role> Roles { get; set; }
    }
}