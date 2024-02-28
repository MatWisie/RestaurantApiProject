using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;
using RestaurantAPI.Models.DTOs;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public OrderController(RestaurantDbContext context)
        {
            _context = context;
        }

        // GET: api/Order
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Worker)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderModel>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        // GET: api/Order/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderModel>> GetOrderModel(int id)
        {
            var orderModel = await _context.Orders.FindAsync(id);

            if (orderModel == null)
            {
                return NotFound();
            }
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (orderModel.IdentityUserId.ToString() == userId || User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Worker))
            {
                return orderModel;
            }
            return Unauthorized();

        }

        [Authorize]
        [HttpGet("/GetAllUserOrders/{id}")]
        public async Task<ActionResult<IEnumerable<OrderModel>>> GetAllUserOrderModel(string id)
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != id.ToString() && !User.IsInRole(UserRoles.Admin) && !User.IsInRole(UserRoles.Worker))
            {
                return Unauthorized();
            }

            var userOrders = await _context.Orders
                            .Where(o => o.IdentityUserId == id)
                            .ToListAsync();

            if (userOrders == null)
            {
                return NotFound();
            }

            return userOrders;

        }

        // PUT: api/Order/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderModel(int id, OrderPostModel orderPostModel)
        {

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != orderPostModel.IdentityUserId.ToString() && !User.IsInRole(UserRoles.Admin) && !User.IsInRole(UserRoles.Worker))
            {
                return Unauthorized();
            }

            var existingOrderModel = await _context.Orders.FindAsync(id);
            if (existingOrderModel == null)
            {
                return NotFound();
            }
            if (id != existingOrderModel.Id)
            {
                return BadRequest();
            }

            if (CheckIfAvailableTable(orderPostModel.TableModelId) == false || CheckIfAvailableDish(orderPostModel.DishModelId) == false)
            {
                return BadRequest();
            }

            existingOrderModel.Status = orderPostModel.Status;
            existingOrderModel.Price = orderPostModel.Price;
            existingOrderModel.TableModelId = orderPostModel.TableModelId;
            existingOrderModel.DishModelId = orderPostModel.DishModelId;
            existingOrderModel.IdentityUserId = orderPostModel.IdentityUserId;

            _context.Entry(existingOrderModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Order
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OrderModel>> PostOrderModel(OrderPostModel orderPostModel)
        {
            if (CheckIfAvailableDish(orderPostModel.DishModelId) == false || CheckIfAvailableTable(orderPostModel.TableModelId) == false)
            {
                return BadRequest("Order or table is not available");
            }

            OrderModel orderModel = new OrderModel()
            {
                Status = orderPostModel.Status,
                Price = orderPostModel.Price,
                TableModelId = orderPostModel.TableModelId,
                DishModelId = orderPostModel.DishModelId,
                IdentityUserId = orderPostModel.IdentityUserId,
            };
            _context.Orders.Add(orderModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrderModel", new { id = orderModel.Id }, orderModel);
        }

        // DELETE: api/Order/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderModel(int id)
        {


            var orderModel = await _context.Orders.FindAsync(id);
            if (orderModel == null)
            {
                return NotFound();
            }

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != orderModel.IdentityUserId.ToString() && !User.IsInRole(UserRoles.Admin) && !User.IsInRole(UserRoles.Worker))
            {
                return Unauthorized();
            }

            _context.Orders.Remove(orderModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderModelExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        private bool CheckIfAvailableTable(int tableModelId)
        {
            var existingTable = _context.Tables.Find(tableModelId);

            if ((existingTable == null || existingTable.IsAvailable == false))
            {
                return false;
            }
            return true;

        }

        private bool CheckIfAvailableDish(int dishModelId)
        {
            var existingDish = _context.Dishes.Find(dishModelId);

            if ((existingDish == null || existingDish.Availability == false))
            {
                return false;
            }
            return true;

        }
    }
}
