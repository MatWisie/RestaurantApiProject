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

    }
}
