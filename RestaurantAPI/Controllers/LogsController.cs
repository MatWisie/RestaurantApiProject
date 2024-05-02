using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly RestaurantDbContext _context;
        public LogsController(RestaurantDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        [Route("login-logs")]
        public async Task<ActionResult<IEnumerable<LoginLogModel>>> GetLoginLogs()
        {
            var logs = await _context.LoginLogs.ToListAsync();
            return Ok(logs);
        }
    }
}
