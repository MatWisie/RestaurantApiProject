using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;
using RestaurantAPI.Models.DTOs;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DishController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public DishController(RestaurantDbContext context)
        {
            _context = context;
        }

        // GET: api/Dish
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DishModel>>> GetDishes()
        {
            var dishes = await _context.Dishes.ToListAsync();
            List<DishGetModel> dishesDto = new List<DishGetModel>();
            foreach (var dish in dishes)
            {
                var tmpDish = new DishGetModel()
                {
                    Id = dish.Id,
                    Name = dish.Name,
                    Description = dish.Name,
                    Price = dish.Price,
                    Availability = dish.Availability
                };
                dishesDto.Add(tmpDish);
            }
            return Ok(dishesDto);
        }

        // GET: api/Dish/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DishGetModel>> GetDishModel(int id)
        {
            var dishModel = await _context.Dishes.FindAsync(id);

            if (dishModel == null)
            {
                return NotFound();
            }
            var dishDto = new DishGetModel()
            {
                Id = dishModel.Id,
                Name = dishModel.Name,
                Description = dishModel.Name,
                Price = dishModel.Price,
                Availability = dishModel.Availability
            };

            return dishDto;
        }

        // PUT: api/Dish/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = UserRoles.Worker + "," + UserRoles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDishModel(int id, DishPostModel dishPostModel)
        {
            var existingDishModel = await _context.Dishes.FindAsync(id);
            if (existingDishModel == null)
            {
                return NotFound();
            }
            existingDishModel.Name = dishPostModel.Name;
            existingDishModel.Description = dishPostModel.Description;
            existingDishModel.Price = dishPostModel.Price;
            existingDishModel.Availability = dishPostModel.Availability;

            _context.Entry(existingDishModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DishModelExists(id))
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

        // POST: api/Dish
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = UserRoles.Worker + "," + UserRoles.Admin)]
        [HttpPost]
        public async Task<ActionResult<DishPostModel>> PostDishModel(DishPostModel dishModel)
        {
            DishModel dish = new DishModel
            {
                Availability = dishModel.Availability,
                Description = dishModel.Description,
                Name = dishModel.Name,
                Price = dishModel.Price,
            };
            _context.Dishes.Add(dish);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDishModel", new { id = dish.Id }, dishModel);
        }

        // DELETE: api/Dish/5
        [Authorize(Roles = UserRoles.Worker + "," + UserRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDishModel(int id)
        {
            var dishModel = await _context.Dishes.FindAsync(id);
            if (dishModel == null)
            {
                return NotFound();
            }

            _context.Dishes.Remove(dishModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DishModelExists(int id)
        {
            return _context.Dishes.Any(e => e.Id == id);
        }
    }
}
