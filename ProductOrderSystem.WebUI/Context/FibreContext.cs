using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using ProductOrderSystem.Domain.Models;
using ProductOrderSystem.Domain.Models.Fibre;
using ProductOrderSystem.Domain.Mapping;

namespace ProductOrderSystem.WebUI.Context
{
    public class FibreContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Order_Fibre> Orders { get; set; }
        public DbSet<OrderFile_Fibre> OrderFiles { get; set; }
        public DbSet<StatusType> StatusTypes { get; set; }
        public DbSet<OrderType> OrderTypes { get; set; }
        public DbSet<ActionType_Fibre> ActionTypes { get; set; }
        public DbSet<OrderAudit_Fibre> OrderAudits { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.Add(new UserMapping());
            modelBuilder.Configurations.Add(new OrderMapping());
            base.OnModelCreating(modelBuilder);
        }
    }
}