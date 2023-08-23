using Health_Tracking_DataService.IConfiguration;
using Health_Tracking_DataService.IRepository;
using Health_Tracking_DataService.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health_Tracking_DataService.Data
{
    public class UnitOfWork : IUnitOfWork,IDisposable
    {
        private readonly AppDbContext _context;

        private readonly ILogger _logger;

        public IUsersRepository Users { get; private set; }

        public UnitOfWork(AppDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger("db_Logs");

            Users = new UsersRepository(_context, _logger);
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
