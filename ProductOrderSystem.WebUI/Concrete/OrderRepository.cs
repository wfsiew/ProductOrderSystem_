using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ProductOrderSystem.Domain.Fibre.Models;
using ProductOrderSystem.WebUI.Abstract;
using ProductOrderSystem.WebUI.Context;

namespace ProductOrderSystem.WebUI.Concrete
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

        public IQueryable<Order> Orders
        {
            get
            {
                return context.Orders;
            }
        }

        public IQueryable<OrderFile> OrderFiles
        {
            get
            {
                return context.OrderFiles;
            }
        }

        public void InsertOrderFile(OrderFile o)
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

        public void DeleteOrderFile(OrderFile o)
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

        public void Insert(Order o)
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

        public void Delete(Order o)
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

        public void Update(Order o)
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

        public void InsertOrderAudit(OrderAudit o)
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