using Health_Tracking_API.Controllers.v1;
using Health_Tracking_DataService.Data;
using Health_Tracking_DataService.IConfiguration;
using Health_Tracking_Entities.DbSet;
using Health_Tracking_Entities.Dtos.Incomming;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Health_Tracking_API.Controllers
{
    //Make this controller private and accessible to only those who pass jwt token with the request
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : BaseController
    {
        public UsersController(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        //GetAll
        [HttpGet]
        [HttpHead]
        public async Task<IActionResult> GetUsers() 
        {
            var users = await _UnitOfWork.Users.All();
            return Ok(users);
        }


        //Post

        [HttpPost]
        public async Task<IActionResult> AddUser(UserDto user)
        {
            var _user = new User();

            _user.FirstName = user.FirstName;
            _user.LastName = user.LastName;
            _user.Email = user.Email;
            _user.Status = 1;
            _user.Phone = user.Phone;
            _user.DateOfBirth = Convert.ToDateTime(user.DateOfBirth);
           _user.Country = user.Country;

            await _UnitOfWork.Users.Add(_user);
            _UnitOfWork.CompleteAsync();

            //have to provide a route to access a created object which is GetUser
            return CreatedAtRoute("GetUser",new { id = _user.Id},user);
        }

        //Get
        [HttpGet]
        [Route("GetUser", Name = "GetUser")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _UnitOfWork.Users.GetById(id);
            return Ok(user);
        }
    }
}
