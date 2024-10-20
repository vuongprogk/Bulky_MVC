using System.Linq.Expressions;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T>
        where T : class
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
            _context = db;
            dbSet = _context.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public void Delete(T entity)
        {
            dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeProperties">CoverType</param>
        /// <returns></returns>
        public IEnumerable<T> GetAll(string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }))
                {
                    query = query.Include(includeProp);
                }

            }
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> fillter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            query = query.Where(fillter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }))
                {
                    query = query.Include(includeProp);
                }

            }
            return query.FirstOrDefault();
        }
    }
}
