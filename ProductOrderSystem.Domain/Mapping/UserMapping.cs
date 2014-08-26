using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.ModelConfiguration;
using ProductOrderSystem.Domain.Models;

namespace ProductOrderSystem.Domain.Mapping
{
    public class UserMapping : EntityTypeConfiguration<User>
    {
        public UserMapping()
        {
            HasMany(i => i.Roles)
                .WithMany(u => u.Users)
                .Map(m =>
                {
                    m.ToTable("UserRole");
                    m.MapLeftKey("UserID");
                    m.MapRightKey("RoleID");
                });

            HasMany(i => i.Teams)
                .WithMany(u => u.Users)
                .Map(m =>
                {
                    m.ToTable("UserTeam");
                    m.MapLeftKey("UserID");
                    m.MapRightKey("TeamID");
                });
        }
    }
}
