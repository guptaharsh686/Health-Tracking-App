using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health_Tracking_Entities.DbSet
{
    public class RefreshToken : BaseEntity
    {
        public string UserId { get; set; } // USerId when logged in
        public string Token { get; set; } 
        public string JwtId { get; set; } // Id generated when Jwd Id has been requested

        public bool IsUsed { get; set; } // To make sure that token is only used once

        public bool IsRevoked { get; set; } // To make sure they are valid

        public DateTime ExpiryDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }
    }
}
