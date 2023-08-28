using Health_Tracking_DataService.IConfiguration;
using Health_Tracking_Entities.Dtos.Incomming.Profile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Health_Tracking_API.Controllers.v1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProfileController : BaseController
    {
        public ProfileController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager) : base(unitOfWork,userManager)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var loggedInUser = await _userManager.GetUserAsync(HttpContext.User); // E5 25:00 for explaination
            
            if(loggedInUser == null) 
            {
                return BadRequest("User not Found");
            }

            var idetityId = new Guid(loggedInUser.Id);

            var profile = await _UnitOfWork.Users.GetByIdentityId(idetityId);

            if (profile == null)
            {
                return BadRequest("User not Found");
            }

            return Ok(profile);
        }


        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto profileDto)
        {
            // If the model is valid
            if(!ModelState.IsValid) 
            {
                return BadRequest("Invalid PAyload");
            }

            var loggedInUser = await _userManager.GetUserAsync(HttpContext.User);

            if (loggedInUser == null)
            {
                return BadRequest("User not Found");
            }

            var idetityId = new Guid(loggedInUser.Id);

            var Userprofile = await _UnitOfWork.Users.GetByIdentityId(idetityId);

            if (Userprofile == null)
            {
                return BadRequest("User not Found");
            }

            Userprofile.Address = profileDto.Address;
            Userprofile.Sex = profileDto.Sex;
            Userprofile.Country = profileDto.Country;
            Userprofile.MobileNumber = profileDto.MobileNumber;

            var isUpdated = await _UnitOfWork.Users.UpdateUserProfile(Userprofile);

            if (isUpdated) 
            {
                await _UnitOfWork.CompleteAsync();
                return Ok(Userprofile);
            }

            return BadRequest("Something went wrong please try again later");


        }
    }
}
