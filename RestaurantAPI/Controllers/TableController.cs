using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;
using RestaurantAPI.Models.DTOs;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly RestaurantDbContext _context;
        private readonly InfrastructureService _infraService;

        public TableController(RestaurantDbContext context)
        {
            _context = context;
            _infraService = new InfrastructureService();

            _infraService.EnsureInfrastructureJsonExists();
        }

        [HttpGet("get-infrastructure")]
        public ActionResult<InfrastructureModel> GetInfrastructure()
        {
            try
            {
                var infraModel = _infraService.GetInfrastructureModel();
                return Ok(infraModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = UserRoles.Worker + "," + UserRoles.Admin)]
        [HttpPost("edit-infrastructure")]
        public ActionResult<InfrastructureModel> EditInfrastructure(InfrastructureModel infrastructureModel)
        {
            bool result = _infraService.EditInfrastructure(infrastructureModel);
            if (result)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<TableGetModel>>> GetTables()
        {
            var tables = await _context.Tables.ToListAsync();
            List<TableGetModel> tableDto = new List<TableGetModel>();
            foreach (var table in tables)
            {
                var tmpTable = new TableGetModel()
                {
                    Id = table.Id,
                    IsAvailable = table.IsAvailable,
                    NumberOfSeats = table.NumberOfSeats,
                    GridRow = table.GridRow,
                    GridColumn = table.GridColumn
                };
                tableDto.Add(tmpTable);
            }
            return Ok(tableDto);
        }

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = UserRoles.Worker + "," + UserRoles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTableModel(int id, TablePostModel tablePostModel)
        {
            if (tablePostModel.GridColumn <= 0 || tablePostModel.GridRow <= 0 || tablePostModel.NumberOfSeats <= 0)
            {
                return BadRequest("Wrong table model");
            }

            if (!CheckIfProperGrid(tablePostModel))
            {
                return BadRequest("Restaurant does not have this many rows or columns");
            }

            var existingTableModel = await _context.Tables.FindAsync(id);
            if (existingTableModel == null)
            {
                return NotFound();
            }

            existingTableModel.IsAvailable = tablePostModel.IsAvailable;
            existingTableModel.NumberOfSeats = tablePostModel.NumberOfSeats;
            existingTableModel.GridRow = tablePostModel.GridRow;
            existingTableModel.GridColumn = tablePostModel.GridColumn;

            _context.Entry(existingTableModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TableModelExists(id))
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

        [Authorize(Roles = UserRoles.Worker + "," + UserRoles.Admin)]
        [HttpPut("/ChangeAvailability/{id}")]
        public async Task<IActionResult> ChangeTableAvailability(int id)
        {
            var existingTableModel = await _context.Tables.FindAsync(id);
            if (existingTableModel == null)
            {
                return NotFound();
            }

            existingTableModel.IsAvailable = !existingTableModel.IsAvailable;

            _context.Entry(existingTableModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TableModelExists(id))
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

        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = UserRoles.Worker + "," + UserRoles.Admin)]
        [HttpPost]
        public async Task<ActionResult<TablePostModel>> PostTableModel(TablePostModel tableModel)
        {
            if (tableModel.GridColumn <= 0 || tableModel.GridRow <= 0 || tableModel.NumberOfSeats <= 0)
            {
                return BadRequest("Wrong table model");
            }

            if (!CheckIfProperGrid(tableModel))
            {
                return BadRequest("Restaurant does not have this many rows or columns");
            }

            var existingTableInPosition = _context.Tables.Any(e => e.GridRow == tableModel.GridRow && e.GridColumn == tableModel.GridColumn);

            if (existingTableInPosition)
            {
                return BadRequest("Table already exist in this position");
            }


            TableModel table = new TableModel()
            {
                IsAvailable = tableModel.IsAvailable,
                NumberOfSeats = tableModel.NumberOfSeats,
                GridRow = tableModel.GridRow,
                GridColumn = tableModel.GridColumn,
            };
            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            return Ok(tableModel);
        }

        [Authorize(Roles = UserRoles.Worker + "," + UserRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTableModel(int id)
        {
            var tableModel = await _context.Tables.FindAsync(id);
            if (tableModel == null)
            {
                return NotFound();
            }

            _context.Tables.Remove(tableModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TableModelExists(int id)
        {
            return _context.Tables.Any(e => e.Id == id);
        }

        private bool CheckIfProperGrid(TablePostModel tablePostModel)
        {
            var infModel = _infraService.GetInfrastructureModel();

            if (tablePostModel.GridColumn > infModel.NumberOfColumns || tablePostModel.GridRow > infModel.NumberOfRows)
            {
                return false;
            }
            return true;
        }
    }
}
