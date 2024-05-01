using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RestaurantAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace RestaurantAPI.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly RestaurantDbContext _context;

        public AuthController(UserManager<IdentityUserModel> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, RestaurantDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;

            CreateDefaultAdmin().Wait();
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("login-worker")]
        public async Task<IActionResult> LoginWorker([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {

                if (await CheckForLockout(user))
                {
                    await AddLoginLog(false, HttpStatusCode.Forbidden, user.Id);
                    return Unauthorized("Your account is locked. Please try again later");
                    //return StatusCode(StatusCodes.Status403Forbidden, new Response { Status = "Error", Message = "Your account is locked. Please try again later" });
                }

                if (await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (!userRoles.Any(e => e.Contains(UserRoles.Admin) || e.Contains(UserRoles.Worker)))
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Selected user is not worker or admin" });
                    }

                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                    };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    var token = GetToken(authClaims);
                    bool originalFirstTimeLoggedIn = user.FirstTimeLoggedIn;

                    if (user.FirstTimeLoggedIn)
                    {
                        user.FirstTimeLoggedIn = false;
                        await _userManager.UpdateAsync(user);
                    }

                    await AddLoginLog(true, HttpStatusCode.OK, user.Id);

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo,
                        userId = user.Id,
                        userRoles = userRoles,
                        firstLogin = originalFirstTimeLoggedIn
                    });
                }
                else
                {
                    await _userManager.AccessFailedAsync(user);
                    await AddLoginLog(false, HttpStatusCode.Unauthorized, user.Id);
                    return Unauthorized("Wrong name or password");
                }
            }
            await AddLoginLog(false, HttpStatusCode.Unauthorized, null);
            return Unauthorized("Wrong name or password");
        }

        [HttpPost]
        [Route("login-user")]
        public async Task<IActionResult> LoginUser([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                if (await CheckForLockout(user))
                {
                    await AddLoginLog(false, HttpStatusCode.Forbidden, user.Id);
                    return Unauthorized("Your account is locked. Please try again later");
                }

                if (await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (userRoles.Any(e => e == UserRoles.Admin || e == UserRoles.Worker))
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Selected user is not worker or admin" });
                    }

                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                    };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    var token = GetToken(authClaims);

                    await AddLoginLog(true, HttpStatusCode.OK, user.Id);

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo,
                        userId = user.Id,
                        userRoles = userRoles
                    });
                }
                else
                {
                    await _userManager.AccessFailedAsync(user);
                    await AddLoginLog(false, HttpStatusCode.Unauthorized, user.Id);
                    return Unauthorized("Wrong name or password");
                }
            }
            await AddLoginLog(false, HttpStatusCode.Unauthorized, null);
            return Unauthorized("Wrong name or password");
        }

        [HttpPost]
        [Route("register-user")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            var passwordPattern = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");

            if (!passwordPattern.IsMatch(model.Password))
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character." });

            IdentityUserModel user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                FirstTimeLoggedIn = false,
                Age = model.Age
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await _roleManager.RoleExistsAsync(UserRoles.User))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [Route("register-worker")]
        public async Task<IActionResult> RegisterWorker([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            var passwordPattern = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");

            if (!passwordPattern.IsMatch(model.Password))
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character." });

            IdentityUserModel user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                FirstTimeLoggedIn = true,
                Age = model.Age,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            if (!await _roleManager.RoleExistsAsync(UserRoles.Worker))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Worker));

            if (await _roleManager.RoleExistsAsync(UserRoles.Worker))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Worker);
            }

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        [HttpGet]
        [Authorize]
        [Route("get-username")]
        public IActionResult GetUser()
        {
            return Ok(User.Identity.Name);
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        private async Task CreateDefaultAdmin()
        {
            var existingAdmin = await _userManager.GetUsersInRoleAsync(UserRoles.Admin);
            if (existingAdmin.Count >= 1)
            {
                return;
            }

            IdentityUserModel user = new()
            {
                Email = "defaultAdmin@admin.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = "Admin"
            };
            var result = await _userManager.CreateAsync(user, "Admin123!");
            if (!result.Succeeded)
                return;

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }
        }

        private async Task<bool> CheckForLockout(IdentityUserModel user)
        {
            var isLockedOut = await _userManager.IsLockedOutAsync(user);
            return isLockedOut;
        }

        private async Task AddLoginLog(bool success, HttpStatusCode statusCode, string? userId)
        {
            string clientIP = GetClientIpAddress(Request);
            LoginLogModel log = new LoginLogModel()
            {
                IpAddress = clientIP,
                Success = success,
                CreatedAt = DateTime.Now,
                StatusCode = statusCode,
                IdentityUserId = userId
            };
            _context.LoginLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        private string GetClientIpAddress(HttpRequest request)
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress;
            if (remoteIp != null && remoteIp.IsIPv4MappedToIPv6)
            {
                return remoteIp.MapToIPv4().ToString();
            }
            else return remoteIp.ToString();
        }
    }
}
