using Health_Tracking_DataService.Data;
using Health_Tracking_Entities.DbSet;
using Health_Tracking_Entities.Dtos.Incomming;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Health_Tracking_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private AppDbContext _context;
        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        //GetAll
        [HttpGet]
        public IActionResult GetUsers() 
        {
            var users = _context.Users.Where(x => x.Status == 1).ToList();
            return Ok(users);
        }


        //Post

        [HttpPost]
        public IActionResult AddUser(UserDto user)
        {
            var _user = new User();

            _user.FirstName = user.FirstName;
            _user.LastName = user.LastName;
            _user.Email = user.Email;
            _user.Status = 1;
            _user.Phone = user.Phone;
            _user.DateOfBirth = Convert.ToDateTime(user.DateOfBirth);
           _user.Country = user.Country;

            _context.Users.Add(_user);
            _context.SaveChanges();

            //will return 201 in future
            return Ok();
        }

        //Get
        [HttpGet]
        [Route("GetUser")]
        public IActionResult GetUser(Guid id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            return Ok(user);
        }
    }
}
