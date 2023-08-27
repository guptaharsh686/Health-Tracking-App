using Health_Tracking_Authentication.Configuration;
using Health_Tracking_Authentication.Models.DTO.Incomming;
using Health_Tracking_Authentication.Models.DTO.Outgoing;
using Health_Tracking_DataService.IConfiguration;
using Health_Tracking_Entities.DbSet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Health_Tracking_API.Controllers.v1
{
    public class AccountsController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        public AccountsController(
            IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            IOptionsMonitor<JwtConfig> optionsMonitor
            ) : base(unitOfWork)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        //Register Action
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistreationDto registreationDto)
        {
            //check the model or Obj if it is valid
            // valid means all the data types or required values are sent
            if (ModelState.IsValid)
            {
                //check if email already exists
                var userExists = await _userManager.FindByEmailAsync(registreationDto.Email);

                if (userExists != null) // Email is already in the table
                {
                    return BadRequest(new UserRegisterationResponseDto
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Email Already in use"
                        }
                    });
                }

                //Add the user if email check fails
                var newUSer = new IdentityUser()
                {
                    Email = registreationDto.Email,
                    UserName = registreationDto.Email,
                    EmailConfirmed = true,    // Add email functionality to confirm users email
                };


                //Add the user to users table
                var isCreated = await _userManager.CreateAsync(newUSer, registreationDto.Password);
                if (!isCreated.Succeeded) // when the registeration has failed
                {
                    return BadRequest(new UserRegisterationResponseDto
                    {
                        Success = isCreated.Succeeded,
                        Errors = isCreated.Errors.Select(x => x.Description).ToList()
                    });
                }

                //create a jwt token
                var token = GenerateJwtToken(newUSer);

                //return back to user
                return Ok(new UserRegisterationResponseDto
                {
                    Success = true,
                    Token = token,
                });
            }
            else // Invalid 
            {
                return BadRequest(new UserRegisterationResponseDto
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Invalid PAyload"
                    }
                });
            }
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            //handler is going to be responsible to create a token
            var jwtHandler = new JwtSecurityTokenHandler();
            //Get the security key
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id), // sub = unique id
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // unique identifier for jwt token specific to tokens . USed by refresh token
                }),
                Expires = DateTime.UtcNow.AddHours(3), // To Update the expiration time to minutes
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature), // To review the algo after
            };

            //generated the sequrity token
            var token = jwtHandler.CreateToken(tokenDescriptor);

            //convert seq object token into string
            var jwtToken = jwtHandler.WriteToken(token);

            return jwtToken;
        }
    }
}
