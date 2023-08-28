using Health_Tracking_Authentication.Configuration;
using Health_Tracking_Authentication.Models.DTO.Generic;
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
        private readonly JwtConfig _jwtConfig;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public AccountsController(
            IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            TokenValidationParameters tokenValidationParameters,
            IOptionsMonitor<JwtConfig> optionsMonitor
            ) : base(unitOfWork,userManager)
        {
            _jwtConfig = optionsMonitor.CurrentValue;
            _tokenValidationParameters = tokenValidationParameters;
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


                //Add to custom users table database
                var _user = new User();

                _user.FirstName = registreationDto.FirstName;
                _user.LastName = registreationDto.LastName;
                _user.Email = registreationDto.Email;
                _user.Status = 1;
                _user.Phone = "";
                _user.DateOfBirth = DateTime.UtcNow;
                _user.Country = "";
                _user.IdentityId = new Guid (newUSer.Id);


                await _UnitOfWork.Users.Add(_user);
                await _UnitOfWork.CompleteAsync();

                //create a jwt token
                var token = await GenerateJwtToken(newUSer);

                //return back to user
                return Ok(new UserRegisterationResponseDto
                {
                    Success = true,
                    Token = token.JWTToken,
                    RefreshToken = token.RefreshToken
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


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto loginRequestDto)
        {
            if (ModelState.IsValid)
            {

                //check if email exist
                var userExists = await _userManager.FindByEmailAsync(loginRequestDto.Email);

                if(userExists == null) 
                {
                    return BadRequest(new UserLoginResponseDto
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Invalid Authentication Request"
                        }
                    });
                }

                //check if user has valid password
                var isCorrect = await _userManager.CheckPasswordAsync(userExists, loginRequestDto.Password);

                if(isCorrect) 
                {
                    //generate a JWT token
                    var jwtToken = await GenerateJwtToken(userExists);

                    return Ok(new UserLoginResponseDto
                    {
                        Success = true,
                        Token = jwtToken.JWTToken,
                        RefreshToken = jwtToken.RefreshToken

                    });
                }
                else
                {
                    return BadRequest(new UserLoginResponseDto
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "PAssword dosent Match"
                        }
                    });
                }

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


        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequestDto)
        {
            if(ModelState.IsValid) 
            {
                // Check if the token is valid

                var result = await VerifyToken(tokenRequestDto);

                if(result == null)
                {
                    return BadRequest(new UserRegisterationResponseDto
                    {
                        Success = false,
                        Errors = new List<string>()
                    {
                        "Token Validation Failed"
                    }
                    });
                }

                return Ok(result);
            }
            else
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

        private async Task<AuthResult> VerifyToken(TokenRequestDto tokenRequestDto)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                //We need to check the validity of this token
                var principal = tokenHandler.ValidateToken(tokenRequestDto.Token,_tokenValidationParameters,out var validatedToken);
                
                
                // We need to validate the results that has been generated for us
                // Validate if the string is an actual JWT Token not a random string 
                if(validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    //check if the JWT token is created with the same algo as our JWT Token
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                    if(!result)
                    {
                        return null;
                    }
                }

                //We need to check the expiry date of token

                var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                //Convert to date to check
                var ExpiryDate = UnixTimeStampToDateTime(utcExpiryDate);

                if(ExpiryDate > DateTime.UtcNow) // Refresh token has actually expired
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Jwt Token has not expired"
                        }
                    };
                }

                //check if refresh token exist
                var refreshTokenExist = await _UnitOfWork.RefreshTokens.GetByRefreshToken(tokenRequestDto.RefreshToken);
                if(refreshTokenExist == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Invalid Refresh Token"
                        }
                    };
                }

                //Check the expiry date of refresh token
                if(refreshTokenExist.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Refresh Token has expired please login again"
                        }
                    };
                }

                //Check if refresh token has been used or not
                if (refreshTokenExist.IsUsed)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Refresh Token has been used"
                        }
                    };
                }


                //Check if refresh token has been revoked
                if (refreshTokenExist.IsRevoked)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Refresh Token has been revoked and cannot be used"
                        }
                    };
                }

                var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if(refreshTokenExist.JwtId != jti)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Refresh Token reference dosenot match the jwt token"
                        }
                    };
                }

                //Start processing and get new token
                refreshTokenExist.IsUsed = true;
                var updateResult = await _UnitOfWork.RefreshTokens.MarkRefreshTokenUsed(refreshTokenExist);
                if(updateResult)
                {
                    await _UnitOfWork.CompleteAsync();

                    //Get the user to generate a JWT token
                    var dbUSer = await _userManager.FindByIdAsync(refreshTokenExist.UserId);
                    if(dbUSer == null)
                    {
                        return new AuthResult
                        {
                            Success = false,
                            Errors = new List<string>()
                        {
                            "Error Processing Request"
                        }
                        };
                    }

                    //Generate a JWT token
                    var tokens = await GenerateJwtToken(dbUSer);
                    return new AuthResult
                    {
                        Token = tokens.JWTToken,
                        Success = true,
                        RefreshToken = tokens.RefreshToken,
                    };
                }

                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string>()
                        {
                            "Error Processing Request"
                        }
                };

            }
            catch (Exception ex)
            {

                //Add better error handling
                //Add logger 
                return null;
            }
        }

        private DateTime UnixTimeStampToDateTime(long unixDate)
        {
            //Sets the time to 1st jan 1970
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            //Add no of secs from 1st jan 1970
            dateTime = dateTime.AddSeconds(unixDate).ToUniversalTime();

            return dateTime;
        }

        private async Task<TokenData> GenerateJwtToken(IdentityUser user)
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
                    new Claim(ClaimTypes.NameIdentifier,user.Id), // to konw which token belong to which user
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id), // sub = unique id
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // unique identifier for jwt token specific to tokens . USed by refresh token
                }),
                Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTimeFrame), // To Update the expiration time to minutes
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature), // To review the algo after
            };

            //generated the sequrity token
            var token = jwtHandler.CreateToken(tokenDescriptor);

            //convert seq object token into string
            var jwtToken = jwtHandler.WriteToken(token);

            //Generate a refresh token
            var refreshToken = new RefreshToken
            {
                AddedDate = DateTime.UtcNow,
                Token = $"{RandomStringGenerator(25)}_{Guid.NewGuid()}",
                UserId = user.Id,
                IsRevoked = false,
                IsUsed = false,
                Status = 1,
                JwtId = token.Id,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),

            };

            await _UnitOfWork.RefreshTokens.Add(refreshToken);
            await _UnitOfWork.CompleteAsync();

            var tokenData = new TokenData
            {
                JWTToken = jwtToken,
                RefreshToken = refreshToken.Token
            };

            return tokenData;
        }


        private string RandomStringGenerator(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
