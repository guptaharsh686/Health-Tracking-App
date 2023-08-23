using Health_Tracking_DataService.Data;
using Health_Tracking_DataService.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health_Tracking_DataService.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        //protected to make it available to child classes which will implement/inherit it
        protected AppDbContext _context;
        protected readonly ILogger _logger;

        //To map to the current table of entity type
        //Ef will know that which tebla we have to perform actions of add,All,Delete
        internal DbSet<T> dbSet;

        public GenericRepository(AppDbContext context,ILogger logger)
        {
            _context = context;
            _logger = logger;
            //initializing to current Table in entity framework
            dbSet = context.Set<T>();
        }
        public virtual async Task<bool> Add(T entity)
        {
            await dbSet.AddAsync(entity);
            return true;
        }

        public virtual async Task<IEnumerable<T>> All()
        {
            var entities = await dbSet.ToListAsync();
            return entities;
        }

        public virtual Task<bool> Delete(Guid id, string userId)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<T> GetById(Guid id)
        {
            return await dbSet.FindAsync(id);
        }

        //not implementing here because this can be entity specific function
        public Task<bool> Upsert(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
