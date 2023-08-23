using Health_Tracking_DataService.Data;
using Health_Tracking_DataService.IRepository;
using Health_Tracking_Entities.DbSet;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health_Tracking_DataService.Repository
{
    public class UserRepository : GenericRepository<User>,IUserRepository
    {
        public UserRepository(AppDbContext context,ILogger logger) : base(context,logger)
        {
            
        }


    }
}
