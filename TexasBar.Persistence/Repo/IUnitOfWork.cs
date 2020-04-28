using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TexasBar.Persistence.Context;

namespace TexasBar.Persistence.Repo
{
    public interface IUnitOfWork : IDisposable
    {
        int Save(string userid);
        ITexasDBContext Context { get; }
    }
}
