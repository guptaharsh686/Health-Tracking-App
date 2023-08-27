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
    public class RefreshTokensRepository : GenericRepository<RefreshToken>,IRefreshTokenRepository
    {
        public RefreshTokensRepository(AppDbContext context, ILogger logger) : base(context, logger)
        {

        }

        public override async Task<IEnumerable<RefreshToken>> All()
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
                return Enumerable.Empty<RefreshToken>();
            }
        }
    }
}
