using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ProductOrderSystem.Domain.Models
{
    public class BaseUser
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserEmail { get; set; }

        [MaxLength(20)]
        public string PhoneNo { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public Byte Status { get; set; }
    }

    public class User : BaseUser
    {
        public User()
        {
            Roles = new List<Role>();
        }

        public virtual ICollection<Role> Roles { get; set; }

        public string[] ArrRoles
        {
            get
            {
                return (from role in Roles select role.Name).ToArray();
            }
        }
    }

    public class Role
    {
        public Role()
        {
            Users = new List<User>();
        }

        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
