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
        IQueryable<Order_Fibre> Orders { get; }
        IQueryable<OrderFile_Fibre> OrderFiles { get; }

        void InsertOrderFile(OrderFile_Fibre o);
        void DeleteOrderFile(OrderFile_Fibre o);
        void Insert(Order_Fibre o);
        void Delete(Order_Fibre o);
        void Update(Order_Fibre o);
        void InsertOrderAudit(OrderAudit_Fibre o);
        void Save();
    }
}
