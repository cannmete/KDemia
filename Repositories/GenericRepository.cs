using Microsoft.EntityFrameworkCore;
using KDemia.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace KDemia.Repositories
{
    public class GenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        private readonly DbSet<T> _object;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _object = _context.Set<T>();
        }

        
        
        
        public List<T> GetAll(params string[] includes)
        {
            var query = _object.AsQueryable();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return query.ToList();
        }
        

        
        public List<T> GetAll(Expression<Func<T, bool>> filter)
        {
            return _object.Where(filter).ToList();
        }

        public T Get(Expression<Func<T, bool>> filter)
        {
            return _object.FirstOrDefault(filter);
        }

        public void Add(T entity)
        {
            _object.Add(entity);
            _context.SaveChanges();
        }

        public void Update(T entity)
        {
            _object.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(T entity)
        {
            _object.Remove(entity);
            _context.SaveChanges();
        }
    }
}