using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProductOrderSystem.Domain.Models;
using ProductOrderSystem.Domain.Fibre.Models;
using ProductOrderSystem.WebUI.Context;

namespace ProductOrderSystem.WebUI.Abstract
{
    public interface IOrderRepository : IDisposable
    {
        FibreContext Context { get; }
        IQueryable<Order> Orders { get; }
        IQueryable<OrderFile> OrderFiles { get; }

        void InsertOrderFile(OrderFile o);
        void DeleteOrderFile(OrderFile o);
        void Insert(Order o);
        void Delete(Order o);
        void Update(Order o);
        void InsertOrderAudit(OrderAudit o);
        void Save();
    }
}
