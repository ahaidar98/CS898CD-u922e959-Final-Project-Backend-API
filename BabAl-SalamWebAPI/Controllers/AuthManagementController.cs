using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BabAl_SalamWebAPI.Models;
using System;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;

namespace BabAl_SalamWebAPI.Controllers
{
    [EnableCors("AllowReactFEBabAl-Salam")]
    [Route("api/[controller]")] // api/authmanagement
    [ApiController]
    public class AuthManagementController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtConfig _jwtConfig;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly ApiDbContext _context;

        public AuthManagementController(
            UserManager<IdentityUser> userManager,
            IOptionsMonitor<JwtConfig> optionsMonitor,
            TokenValidationParameters tokenValidationParameters,
            RoleManager<IdentityRole> roleManger,
            ApiDbContext context
            )
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
            _tokenValidationParameters = tokenValidationParameters;
            _roleManager = roleManger;
            _context = context;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDTO userRegistrationDTO)
        {
            // Check if the incoming request is valid
            if (ModelState.IsValid)
            {
                // check i the user with the same email exist
                var existingUser = await _userManager.FindByEmailAsync(userRegistrationDTO.Email);

                if (existingUser != null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Success = false,
                        Errors = new List<string>(){
                            "Email already exist."
                        }
                    });
                }

                if(!await _roleManager.RoleExistsAsync(userRegistrationDTO.Role))
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Success = false,
                        Errors = new List<string>(){
                            "Invalid payload."
                        }
                    });
                }

                var userRegistration = new UserRegistration
                {
                    Email = userRegistrationDTO.Email,
                    Password = userRegistrationDTO.Password,
                    FirstName = userRegistrationDTO.FirstName,
                    LastName = userRegistrationDTO.LastName,
                    Role = userRegistrationDTO.Role
                };

                var newUser = new IdentityUser() {
                    Email = userRegistration.Email,
                    UserName = userRegistration.Email,
                };

                var isCreated = await _userManager.CreateAsync(newUser, userRegistration.Password);
                var idenUserId = await _userManager.GetUserIdAsync(newUser);

                if (isCreated.Succeeded)
                {
                    var dbUser = new User
                    {
                        Email = userRegistrationDTO.Email,
                        FirstName = userRegistrationDTO.FirstName,
                        LastName = userRegistrationDTO.LastName,
                        Role = userRegistrationDTO.Role,
                        Id = idenUserId
                    };

                    _context.Users.Add(dbUser);

                    var success = await _context.SaveChangesAsync() > 0;
                    var jwtToken = await GenerateJwtToken(newUser);

                    if (success)
                    {
                        return Ok(jwtToken);
                    }
                    else
                    {
                        return BadRequest(new RegistrationResponse()
                        {
                            Success = false,
                            Errors = new List<string>(){
                            "An error occured while creating user."
                        }
                        });
                    }
                } 

                return new JsonResult(new RegistrationResponse()
                {
                    Success = false,
                    Errors = isCreated.Errors.Select(x => x.Description).ToList()
                })

                { StatusCode = 500 };
            }

            return BadRequest(new RegistrationResponse()
            {
                Success = false,
                Errors = new List<string>(){
                    "Invalid payload"
                }
            });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest userLoginRequest)
        {
            if (ModelState.IsValid)
            {
                // check if the user with the same email exist
                var existingUser = await _userManager.FindByEmailAsync(userLoginRequest.Email);

                if (existingUser == null)
                {
                    // We dont want to give to much information on why the request has failed for security reasons
                    return BadRequest(new RegistrationResponse()
                    {
                        Success = false,
                        Errors = new List<string>(){
                            "Email or password is invalid."
                        }
                    });
                }

                // Now we need to check if the user has inputed the right password
                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, userLoginRequest.Password);

                if (isCorrect)
                {
                    var jwtToken = await GenerateJwtToken(existingUser);

                    return Ok(jwtToken);
                }
                else
                {
                    // We dont want to give to much information on why the request has failed for security reasons
                    return BadRequest(new RegistrationResponse()
                    {
                        Success = false,
                        Errors = new List<string>(){
                            "Email or password is invalid."
                        }
                    });
                }
            }

            return BadRequest(new RegistrationResponse()
            {
                Success = false,
                Errors = new List<string>(){
                    "Invalid payload."
                }
            });
        }

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (ModelState.IsValid)
            {
                var res = await VerifyToken(tokenRequest);

                if (res == null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                    "Invalid tokens"
                },
                        Success = false
                    });
                }

                return Ok(res);
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>() {
                "Invalid payload"
            },
                Success = false
            });
        }

        [HttpPost]
        [Route("Role")]
        public async Task<IActionResult> AddRole([FromBody] AddUserRole userRole)
        {
            if (ModelState.IsValid)
            {
                if(await _roleManager.RoleExistsAsync(userRole.Role))
                {
                    return BadRequest(new ResponseResult()
                    {
                        MessageStanding = "red",
                        ResponseMessage = "Role already exists."
                    });
                }

                var role = new IdentityRole();
                role.Name = userRole.Role;

                var result = await _roleManager.CreateAsync(role);

                if(result.Succeeded)
                {
                    return Ok(new ResponseResult()
                    {
                        MessageStanding = "green",
                        ResponseMessage = "Role successfully added."
                    }); ;
                }

                return BadRequest(new ResponseResult()
                {
                    MessageStanding = "red",
                    ResponseMessage = "An error has occured. Try again later."
                        
                });
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>() {
                "Invalid payload"
            },
                Success = false
            });
        }

        private async Task<AuthResult> VerifyToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // This validation function will make sure that the token meets the validation parameters
                // and its an actual jwt token not just a random string
                var principal = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);

                // Now we need to check if the token has a valid security algorithm
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (result == false)
                    {
                        return null;
                    }
                }

                // Will get the time stamp in unix time
                var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                // we convert the expiry date from seconds to the date
                var expDate = UnixTimeStampToDateTime(utcExpiryDate);

                if (expDate > DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "We cannot refresh this since the token has not expired" },
                        Success = false
                    };
                }

                // Check the token we got if its saved in the db
                var storedRefreshToken = await _context.RefreshToken.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);

                if (storedRefreshToken == null)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "refresh token doesnt exist" },
                        Success = false
                    };
                }

                // Check the date of the saved token if it has expired
                if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "token has expired, user needs to relogin" },
                        Success = false
                    };
                }

                // check if the refresh token has been used
                if (storedRefreshToken.IsUsed)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "token has been used" },
                        Success = false
                    };
                }

                // Check if the token is revoked
                if (storedRefreshToken.IsRevoked)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "token has been revoked" },
                        Success = false
                    };
                }

                // we are getting here the jwt token id
                var jti = principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                // check the id that the recieved token has against the id saved in the db
                if (storedRefreshToken.JwtId != jti)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "the token doenst mateched the saved token" },
                        Success = false
                    };
                }

                storedRefreshToken.IsUsed = true;
                _context.RefreshToken.Update(storedRefreshToken);
                await _context.SaveChangesAsync();

                var dbUser = await _userManager.FindByIdAsync(storedRefreshToken.UserId);
                return await GenerateJwtToken(dbUser);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        private async Task<AuthResult> GenerateJwtToken(IdentityUser identityUser)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", identityUser.Id),
                    new Claim(JwtRegisteredClaimNames.Email, identityUser.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, identityUser.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                //Expires = DateTime.UtcNow.Add(_jwtConfig.ExpiryTimeFrame),
                Expires = DateTime.UtcNow.AddSeconds(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);
            var tokenExpiration = DateTime.UtcNow.AddSeconds(30);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                UserId = identityUser.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = tokenExpiration,
                IsRevoked = false,
                Token = RandomString(25) + Guid.NewGuid()
            };

            _context.RefreshToken.Add(refreshToken);
            var success = await _context.SaveChangesAsync() > 0;

            if(success)
            {
                // Not working
                var tempLoggedInUser = await _context.Users.FindAsync(identityUser.Id);

                if (tempLoggedInUser == null)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "An error has occured. Please try again later. Errx105" },
                        Success = false
                    };
                }

                var loggedInUser = UserToDTO(tempLoggedInUser);

                return new AuthResult()
                {
                    Token = jwtToken,
                    TokenExpiration = tokenExpiration.ToLongDateString(),
                    Success = true,
                    RefreshToken = refreshToken.Token,
                    User = loggedInUser
                };
            }

            return new AuthResult()
            {
                Errors = new List<string>() { "An error has occured. Please try again later." },
                Success = false
            };
        }

        private string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static UserDTO UserToDTO(User user) {
            return new UserDTO
            {
                Email = user.Email,
                LastName = user.LastName,
                FirstName = user.FirstName,
                Id = user.Id,
                Role = user.Role
            };
        }
    }
}