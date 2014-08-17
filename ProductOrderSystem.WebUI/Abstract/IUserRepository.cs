using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProductOrderSystem.Domain.Models;
using ProductOrderSystem.WebUI.Context;

namespace ProductOrderSystem.WebUI.Abstract
{
    public interface IUserRepository : IDisposable
    {
        FibreContext Context { get; }
        IQueryable<User> Users { get; }
        IQueryable<Role> Roles { get; }

        void Insert(User o);
        void Delete(User o);
        void Update(User o);
        void UpdateUserRoles(string[] selectedRoles, User user);
        User ValidateUser(string email);
        User GetUser(string email);
        string[] GetRolesForUser(string email);
        void Save();
    }
}