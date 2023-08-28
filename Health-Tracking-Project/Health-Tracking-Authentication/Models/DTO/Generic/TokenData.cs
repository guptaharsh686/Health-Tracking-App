using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health_Tracking_Authentication.Models.DTO.Generic
{
    public class TokenData
    {

        [Required]
        public string JWTToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
