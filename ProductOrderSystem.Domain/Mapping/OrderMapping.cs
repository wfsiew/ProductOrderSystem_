using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.ModelConfiguration;
using ProductOrderSystem.Domain.Fibre.Models;

namespace ProductOrderSystem.Domain.Mapping
{
    public class OrderMapping : EntityTypeConfiguration<Order_Fibre>
    {
        public OrderMapping()
        {
            HasMany(i => i.OrderFiles).WithRequired().HasForeignKey(k => k.OrderID);
        }
    }
}
