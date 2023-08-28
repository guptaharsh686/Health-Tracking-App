using Health_Tracking_Entities.DbSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health_Tracking_DataService.IRepository
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken> GetByRefreshToken(string refreshToken);

        Task<bool> MarkRefreshTokenUsed(RefreshToken refreshToken);
    }
}
