
using System;
using System.Collections.Generic;
//using System.Data.Objects;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TexasBar.Persistence.Repo
{
    public interface IRepository<T> : IDisposable where T : class
    {
        IEnumerable<T> All { get; }
        IEnumerable<T> AllEager(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includes);
        T Find(params object[] id);
        List<T> WhereOne(Expression<Func<T, bool>> predicate);
        T FindOne(Func<T, bool> predicate);
        void Insert(T entity);
        void Update(T entity);
        void Delete(object id);
       // T ExecuteCustomStoredProc<T>(string commandName, SqlParameter param);
       // IEnumerable<T> LoadViaStockProc(string procName, object[] param); // SqlParameter param);//; object[] param);
       
        
    
    }
}
