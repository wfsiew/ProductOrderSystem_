using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ProductOrderSystem.Domain.Models;
using ProductOrderSystem.WebUI.Abstract;
using ProductOrderSystem.WebUI.Context;

namespace ProductOrderSystem.WebUI.Concrete
{
    public class UserRepository : IUserRepository, IDisposable
    {
        private bool disposed = false;
        private FibreContext context;

        public UserRepository()
        {
            context = new FibreContext();
        }

        public FibreContext Context
        {
            get
            {
                return context;
            }
        }

        public IQueryable<User> Users
        {
            get
            {
                return context.Users;
            }
        }

        public IQueryable<Role> Roles
        {
            get
            {
                return context.Roles;
            }
        }

        public void Insert(User o)
        {
            try
            {
                if (o.Roles.Count == 0)
                {
                    throw new Exception("User must have at least one role.");
                }

                int count = (from u in context.Users where u.UserEmail == o.UserEmail select u).Count();
                if (count > 0)
                {
                    User usertemp = (from u in context.Users
                                     where u.UserEmail == o.UserEmail
                                     select u).FirstOrDefault();

                    if (usertemp.Status == 0)
                    {
                        throw new Exception("This user is currently inactive, please contact the R&D to activate back the user.");
                    }

                    throw new Exception("This user already exist in the database.");
                }

                context.Users.Add(o);
            }

            catch (Exception)
            {
                throw;
            }
        }

        public void Delete(User o)
        {
            try
            {
                o.Status = 0;
                context.Entry(o).State = EntityState.Modified;
            }

            catch (Exception)
            {
                throw;
            }
        }

        public void Update(User o)
        {
            try
            {
                if (o.Roles.Count == 0)
                {
                    throw new Exception("User must have at least one role.");
                }

                context.Entry(o).State = EntityState.Modified;
            }

            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateUserRoles(string[] selectedRoles, User user)
        {
            if (selectedRoles == null)
            {
                user.Roles = new List<Role>();
                return;
            }

            if (user.Roles == null)
                user.Roles = new List<Role>();

            HashSet<string> selectedRolesHS = new HashSet<string>(selectedRoles);
            HashSet<int> userRoles = new HashSet<int>(user.Roles.Select(x => x.ID));

            foreach (Role role in Context.Roles)
            {
                if (selectedRolesHS.Contains(role.ID.ToString()))
                {
                    if (!userRoles.Contains(role.ID))
                    {
                        user.Roles.Add(role);
                    }
                }

                else
                {
                    if (userRoles.Contains(role.ID))
                    {
                        user.Roles.Remove(role);
                    }
                }
            }
        }

        public User ValidateUser(string email)
        {
            try
            {
                User user = context.Users
                                .Where(u => (u.UserEmail == email))
                                .FirstOrDefault();
                return user;
            }

            catch (Exception e)
            {
                string message = e.Message;
                return null;
            }
        }

        public User GetUser(string email)
        {
            User user = context.Users.Where(u => u.UserEmail == email).FirstOrDefault();
            return user;
        }

        public string[] GetRolesForUser(string email)
        {
            User user = context.Users.Where(u => u.UserEmail == email).FirstOrDefault();

            if (user != null)
            {
                string[] roles = (from role in user.Roles
                                  select role.Name).ToArray();
                return roles;
            }

            else
            {
                return new string[] { "" };
            }
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }
    }
}