using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;
using RestaurantAPI.Models.DTOs;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace RestaurantAPI.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly RestaurantDbContext _context;
        public UserController(UserManager<IdentityUserModel> userManager, RestaurantDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                List<UserGetModel> userGetModels = new List<UserGetModel>();
                foreach (var user in users)
                {
                    var userRole = await _userManager.GetRolesAsync(user);
                    UserGetModel userGetModel = new UserGetModel()
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Role = userRole.Count > 0 ? userRole[0] : "No Role Assigned"
                    };
                    userGetModels.Add(userGetModel);
                }
                return Ok(userGetModels);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("get-user/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(e => e.Id == id);
                if (user == null)
                    return NotFound("User not found");

                var userRole = await _userManager.GetRolesAsync(user);
                UserGetModel userGetModels = new UserGetModel()
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Role = userRole.Count > 0 ? userRole[0] : "No Role Assigned"
                };

                return Ok(userGetModels);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("edit-user")]
        public async Task<IActionResult> EditUser([FromBody] UserEditModel userEditModel)
        {
            if (User.Claims.FirstOrDefault(e => e.Type == ClaimTypes.NameIdentifier).Value == userEditModel.Id || User.IsInRole(UserRoles.Admin))
            {
                var user = await _userManager.FindByIdAsync(userEditModel.Id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var passwordPattern = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");

                if (!passwordPattern.IsMatch(userEditModel.Password))
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character." });

                user.UserName = userEditModel.Username;
                await _userManager.ChangePasswordAsync(user, user.PasswordHash, userEditModel.Password);
                user.Age = userEditModel.Age;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Ok("User updated successfully");
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            else
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpPost("change-user-password")]
        public async Task<IActionResult> EditUserPassword([FromBody] UserChangePasswordModel userNewPasswordModel)
        {
            if (User.Claims.FirstOrDefault(e => e.Type == ClaimTypes.NameIdentifier).Value == userNewPasswordModel.UserId || User.IsInRole(UserRoles.Admin))
            {
                var user = await _userManager.FindByIdAsync(userNewPasswordModel.UserId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var samePassword = await _userManager.CheckPasswordAsync(user, userNewPasswordModel.Password);
                if (!samePassword)
                {
                    return BadRequest("Current password is incorrect");
                }

                var passwordPattern = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");

                if (!passwordPattern.IsMatch(userNewPasswordModel.Password))
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one special character." });

                await _userManager.ChangePasswordAsync(user, user.PasswordHash, userNewPasswordModel.NewPassword);
                user.FirstTimeLoggedIn = false;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Ok("User updated successfully");
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            else
            {
                return Unauthorized();
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("/delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok("User deleted");
            }
            return BadRequest(result.Errors);
        }
    }
}
