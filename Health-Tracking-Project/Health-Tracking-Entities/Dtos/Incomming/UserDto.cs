using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health_Tracking_Entities.Dtos.Incomming
{
    public class UserDto
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public String DateOfBirth { get; set; }
        public string Country { get; set; }

    }
}
