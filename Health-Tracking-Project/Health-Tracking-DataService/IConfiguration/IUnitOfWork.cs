using Health_Tracking_DataService.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health_Tracking_DataService.IConfiguration
{
    public interface IUnitOfWork
    {
        IUsersRepository Users { get; }

        IRefreshTokenRepository RefreshTokens { get; }

        Task CompleteAsync();
    }
}
