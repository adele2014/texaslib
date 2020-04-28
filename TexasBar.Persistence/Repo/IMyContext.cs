using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasBar.Persistence.Repo
{
    public interface IMyContext : IDisposable
    {
        int SaveChanges(string userid);
    }
}
