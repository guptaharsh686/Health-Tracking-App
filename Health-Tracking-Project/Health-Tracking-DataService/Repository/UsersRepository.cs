using Health_Tracking_DataService.Data;
using Health_Tracking_DataService.IRepository;
using Health_Tracking_Entities.DbSet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health_Tracking_DataService.Repository
{
    public class UsersRepository : GenericRepository<User>,IUsersRepository
    {
        public UsersRepository(AppDbContext context,ILogger logger) : base(context,logger)
        {
            
        }

        public override async Task<IEnumerable<User>> All()
        {
            try
            {
                return await dbSet.Where(x => x.Status == 1)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} All Method has generated error", typeof(UsersRepository));
                return Enumerable.Empty<User>();
            }
        }

        public async Task<bool> UpdateUserProfile(User user)
        {
            try
            {
                var ExistingUser =  await dbSet.Where(x => x.Status == 1 && x.Id == user.Id).FirstOrDefaultAsync();

                if (ExistingUser == null)
                {
                    return false;
                }
                ExistingUser.FirstName = user.FirstName;
                ExistingUser.LastName = user.LastName;
                ExistingUser.MobileNumber = user.MobileNumber;
                ExistingUser.Phone = user.Phone;
                ExistingUser.Sex = user.Sex;
                ExistingUser.UpdateDate = DateTime.UtcNow;
                ExistingUser.Address = user.Address;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} UpdateUserProfile Method has generated error", typeof(UsersRepository));
                return false;
            }
        }

    }
}
