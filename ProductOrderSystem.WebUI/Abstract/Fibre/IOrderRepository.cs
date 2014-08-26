using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProductOrderSystem.Domain.Models;
using ProductOrderSystem.Domain.Models.Fibre;
using ProductOrderSystem.WebUI.Context;

namespace ProductOrderSystem.WebUI.Abstract.Fibre
{
    public interface IOrderRepository : IDisposable
    {
        FibreContext Context { get; }
        IQueryable<OrderFibre> Orders { get; }
        IQueryable<OrderFileFibre> OrderFiles { get; }

        void InsertOrderFile(OrderFileFibre o);
        void DeleteOrderFile(OrderFileFibre o);
        void Insert(OrderFibre o);
        void Delete(OrderFibre o);
        void Update(OrderFibre o);
        void InsertOrderAudit(OrderAuditFibre o);
        void Save();
    }
}
