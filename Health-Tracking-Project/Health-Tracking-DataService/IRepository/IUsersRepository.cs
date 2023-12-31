﻿using Health_Tracking_Entities.DbSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health_Tracking_DataService.IRepository
{
    public interface IUsersRepository : IGenericRepository<User>
    {
        Task<bool> UpdateUserProfile(User user);
        Task<User> GetByIdentityId(Guid identityId);
    }
}
