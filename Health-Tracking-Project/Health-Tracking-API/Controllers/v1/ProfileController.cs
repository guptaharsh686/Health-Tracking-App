using Health_Tracking_DataService.IConfiguration;
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
    }
}
