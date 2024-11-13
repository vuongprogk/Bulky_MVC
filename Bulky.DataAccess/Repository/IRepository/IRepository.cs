using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? fillter = null,string? includeProperties = null);
        T GetFirstOrDefault(Expression<Func<T,bool>> fillter, string? includeProperties = null, bool tracked = false);
        void Delete(T entity);
        void Add(T entity);
        void DeleteRange(IEnumerable<T> entities);
    }
}
