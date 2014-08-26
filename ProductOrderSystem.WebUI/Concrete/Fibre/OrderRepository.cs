using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ProductOrderSystem.Domain.Models.Fibre;
using ProductOrderSystem.WebUI.Abstract.Fibre;
using ProductOrderSystem.WebUI.Context;

namespace ProductOrderSystem.WebUI.Concrete.Fibre
{
    public class OrderRepository : IOrderRepository, IDisposable
    {
        private bool disposed = false;
        private FibreContext context;

        public OrderRepository()
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

        public IQueryable<OrderFibre> Orders
        {
            get
            {
                return context.Orders;
            }
        }

        public IQueryable<OrderFileFibre> OrderFiles
        {
            get
            {
                return context.OrderFiles;
            }
        }

        public void InsertOrderFile(OrderFileFibre o)
        {
            try
            {
                o.UploadDatetime = DateTime.Now;
                context.OrderFiles.Add(o);
            }

            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteOrderFile(OrderFileFibre o)
        {
            try
            {
                context.Entry(o).State = EntityState.Deleted;
            }

            catch (Exception)
            {
                throw;
            }
        }

        public void Insert(OrderFibre o)
        {
            try
            {
                context.Orders.Add(o);
            }

            catch (Exception)
            {
                throw;
            }
        }

        public void Delete(OrderFibre o)
        {
            try
            {
                context.Entry(o).State = EntityState.Deleted;
            }

            catch (Exception)
            {
                throw;
            }
        }

        public void Update(OrderFibre o)
        {
            try
            {
                context.Entry(o).State = EntityState.Modified;
            }

            catch (Exception)
            {
                throw;
            }
        }

        public void InsertOrderAudit(OrderAuditFibre o)
        {
            try
            {
                context.OrderAudits.Add(o);
            }

            catch (Exception)
            {
                throw;
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