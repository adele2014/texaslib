using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TexasBar.Persistence.Context;

namespace TexasBar.Persistence.Repo
{
    
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ITexasDBContext context;
        public UnitOfWork()
        {
            context = new TexasDBContext();
        }
        public UnitOfWork(TexasDBContext context)
        {
            this.context = context;
        }
        public int Save(string userid)
        {
            return context.SaveChanges(userid);
        }
        public ITexasDBContext Context
        {
            get
            {
                return context;
            }
        }
        public void Dispose()
        {
            if (context != null)
                context.Dispose();
        }
    }
}
