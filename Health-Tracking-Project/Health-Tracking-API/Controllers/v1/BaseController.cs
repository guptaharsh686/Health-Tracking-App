using Health_Tracking_DataService.IConfiguration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Health_Tracking_API.Controllers.v1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class BaseController : ControllerBase
    {
        protected IUnitOfWork _UnitOfWork;
        protected UserManager<IdentityUser> _userManager;
        public BaseController(IUnitOfWork unitOfWork,UserManager<IdentityUser> userManager)//AppDbContext context
        {
            //_context = context;
            _UnitOfWork = unitOfWork;
            _userManager = userManager;
        }
    }

}
