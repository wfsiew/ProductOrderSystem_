using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using ProductOrderSystem.Domain.Models.Fibre;
using ProductOrderSystem.Task.Context;

namespace ProductOrderSystem.Task.Concrete
{
    public class OrderRepository : IDisposable
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