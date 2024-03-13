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
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
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
                    Role = userRole[0]
                };
                userGetModels.Add(userGetModel);
            }
            return Ok(userGetModels);
        }

        [Authorize]
        [HttpPost("EditUser")]
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
