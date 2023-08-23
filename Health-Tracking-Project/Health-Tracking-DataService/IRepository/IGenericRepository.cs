using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health_Tracking_DataService.IRepository
{
    public interface IGenericRepository<T> where T : class
    {
        //Get All Entities
        Task<IEnumerable<T>> All();

        //Get Specific Entity based on Id
        Task<T> GetById(Guid id);

        Task<bool> Add(T entity);

        Task<bool> Delete(Guid id,string userId);

        //Update entity or add if id dosenot exist
        Task<bool> Upsert(T entity);
    }
}
