using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using ProductOrderSystem.Domain.Models;
using ProductOrderSystem.Domain.Fibre.Models;
using ProductOrderSystem.Domain.Mapping;

namespace ProductOrderSystem.Task.Context
{
    public class FibreContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderFile> OrderFiles { get; set; }
        public DbSet<StatusType> StatusTypes { get; set; }
        public DbSet<OrderType> OrderTypes { get; set; }
        public DbSet<ActionType> ActionTypes { get; set; }
        public DbSet<OrderAudit> OrderAudits { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Configurations.Add(new UserMapping());
            modelBuilder.Configurations.Add(new OrderMapping());
            base.OnModelCreating(modelBuilder);
        }
    }
}