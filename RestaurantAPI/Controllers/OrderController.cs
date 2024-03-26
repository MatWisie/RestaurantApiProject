﻿using Microsoft.AspNetCore.Authorization;
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
            return await _context.Orders.Select(o => new OrderModel
            {
                Id = o.Id,
                Status = o.Status,
                Price = o.Price,
                DishModels = o.DishModels,
                TableModel = o.TableModel,
                IdentityUserId = o.IdentityUserId
            })
            .ToListAsync();

        }

        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Worker)]
        [HttpGet("GetActiveOrders")]
        public async Task<ActionResult<IEnumerable<OrderModel>>> GetActiveOrders()
        {
            return await _context.Orders.Select(o => new OrderModel
            {
                Id = o.Id,
                Status = o.Status,
                Price = o.Price,
                DishModels = o.DishModels,
                TableModel = o.TableModel,
                IdentityUserId = o.IdentityUserId
            }).Where(e => e.Status != Enums.StatusEnum.Paid)
            .ToListAsync();

        }

        // GET: api/Order/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderModel>> GetOrderModel(int id)
        {
            var orderModel = await _context.Orders.Select(o => new OrderModel
            {
                Id = o.Id,
                Status = o.Status,
                Price = o.Price,
                DishModels = o.DishModels,
                TableModel = o.TableModel,
                IdentityUserId = o.IdentityUserId
            }).FirstAsync(e => e.Id == id);

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
                            .Where(o => o.IdentityUserId == id).Select(o => new OrderModel
                            {
                                Id = o.Id,
                                Status = o.Status,
                                Price = o.Price,
                                DishModels = o.DishModels,
                                TableModel = o.TableModel,
                                IdentityUserId = o.IdentityUserId
                            })
                            .ToListAsync();

            if (userOrders == null)
            {
                return NotFound();
            }

            return userOrders;

        }

        [Authorize]
        [HttpGet("/GetAllActiveUserOrders/{id}")]
        public async Task<ActionResult<IEnumerable<OrderModel>>> GetAllActiveUserOrderModel(string id)
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != id.ToString() && !User.IsInRole(UserRoles.Admin) && !User.IsInRole(UserRoles.Worker))
            {
                return Unauthorized();
            }

            var userOrders = await _context.Orders
                            .Where(o => o.IdentityUserId == id && o.Status != Enums.StatusEnum.Paid).Select(o => new OrderModel
                            {
                                Id = o.Id,
                                Status = o.Status,
                                Price = o.Price,
                                DishModels = o.DishModels,
                                TableModel = o.TableModel,
                                IdentityUserId = o.IdentityUserId
                            })
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

            var dishModels = await _context.Dishes.Where(d => orderPostModel.DishModelsId.Contains(d.Id)).ToListAsync();
            var existingOrderModel = await _context.Orders.FindAsync(id);
            if (existingOrderModel == null)
            {
                return NotFound();
            }
            if (id != existingOrderModel.Id)
            {
                return BadRequest();
            }

            if (CheckIfAvailableTable(orderPostModel.TableModelId) == false || CheckIfAvailableDish(dishModels) == false)
            {
                return BadRequest();
            }

            existingOrderModel.TableModelId = orderPostModel.TableModelId;
            existingOrderModel.DishModels = dishModels;

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
            var dishModels = await _context.Dishes.Where(d => orderPostModel.DishModelsId.Contains(d.Id)).ToListAsync();
            if (CheckIfAvailableDish(dishModels) == false || CheckIfAvailableTable(orderPostModel.TableModelId) == false)
            {
                return BadRequest("Order or table is not available");
            }
            orderPostModel.Status = Enums.StatusEnum.Working;
            OrderModel orderModel = new OrderModel()
            {
                Status = orderPostModel.Status,
                Price = orderPostModel.Price,
                TableModelId = orderPostModel.TableModelId,
                DishModels = dishModels,
                IdentityUserId = orderPostModel.IdentityUserId
            };
            _context.Orders.Add(orderModel);
            await _context.SaveChangesAsync();

            return Ok("Order created");
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
        [Authorize]
        [HttpPut("/ChangeStatus_ReadyToPay/{id}")]
        public async Task<IActionResult> ReadytoPayOrderModel(int id)
        {

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var existingOrderModel = await _context.Orders.FindAsync(id);
            if (existingOrderModel == null)
            {
                return NotFound();
            }
            if (userId != existingOrderModel.IdentityUserId.ToString() && !User.IsInRole(UserRoles.Admin) && !User.IsInRole(UserRoles.Worker))
            {
                return Unauthorized();
            }
            if (id != existingOrderModel.Id)
            {
                return BadRequest();
            }
            existingOrderModel.Status = Enums.StatusEnum.ReadyToPay;

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

        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Worker)]
        [HttpPut("/ChangeStatus_Ready/{id}")]
        public async Task<IActionResult> ReadyOrderModel(int id)
        {

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var existingOrderModel = await _context.Orders.FindAsync(id);
            if (existingOrderModel == null)
            {
                return NotFound();
            }
            if (userId != existingOrderModel.IdentityUserId.ToString() && !User.IsInRole(UserRoles.Admin) && !User.IsInRole(UserRoles.Worker))
            {
                return Unauthorized();
            }
            if (id != existingOrderModel.Id)
            {
                return BadRequest();
            }
            existingOrderModel.Status = Enums.StatusEnum.Ready;

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

        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Worker)]
        [HttpPut("/ChangeStatus_Paid/{id}")]
        public async Task<IActionResult> PaidOrderModel(int id)
        {

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var existingOrderModel = await _context.Orders.FindAsync(id);
            if (existingOrderModel == null)
            {
                return NotFound();
            }
            if (userId != existingOrderModel.IdentityUserId.ToString() && !User.IsInRole(UserRoles.Admin) && !User.IsInRole(UserRoles.Worker))
            {
                return Unauthorized();
            }
            if (id != existingOrderModel.Id)
            {
                return BadRequest();
            }
            if (existingOrderModel.Status != Enums.StatusEnum.ReadyToPay)
            {
                return BadRequest();
            }
            existingOrderModel.Status = Enums.StatusEnum.Paid;

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

        private bool CheckIfAvailableDish(List<DishModel> dishModels)
        {
            foreach (var dish in dishModels)
            {
                var existingDish = _context.Dishes.Find(dish.Id);

                if ((existingDish == null || existingDish.Availability == false))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
