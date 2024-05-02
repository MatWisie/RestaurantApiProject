using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;
using RestaurantAPI.Models.DTOs;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public ReservationController(RestaurantDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Worker)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationGetModel>>> GetReservations()
        {
            var reservations = await _context.Reservations.ToListAsync();
            List<ReservationGetModel> reservationsDto = new List<ReservationGetModel>();
            foreach (var res in reservations)
            {
                var tmpRes = new ReservationGetModel()
                {
                    Id = res.Id,
                    From = res.From,
                    To = res.To,
                    IdentityUserId = res.IdentityUserId,
                    TableModelId = res.TableModelId,
                };
                reservationsDto.Add(tmpRes);
            }
            return Ok(reservationsDto);
        }

        [Authorize]
        [HttpGet("/GetAllUserReservations/{id}")]
        public async Task<ActionResult<IEnumerable<ReservationGetModel>>> GetUserReservations(string id)
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != id.ToString() && !User.IsInRole(UserRoles.Admin) && !User.IsInRole(UserRoles.Worker))
            {
                return Unauthorized();
            }

            var reservations = await _context.Reservations
                           .Where(o => o.IdentityUserId == id).ToListAsync();

            List<ReservationGetModel> reservationsDto = new List<ReservationGetModel>();
            foreach (var res in reservations)
            {
                var tmpRes = new ReservationGetModel()
                {
                    Id = res.Id,
                    From = res.From,
                    To = res.To,
                    IdentityUserId = res.IdentityUserId,
                    TableModelId = res.TableModelId,
                };
                reservationsDto.Add(tmpRes);
            }
            return Ok(reservationsDto);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<OrderModel>> PostReservationModel(ReservationPostModel reservationPostModel)
        {
            var existingReservation = await _context.Reservations
                                    .FirstOrDefaultAsync(r =>
                                    r.TableModelId == reservationPostModel.TableModelId &&
                                    ((r.From >= reservationPostModel.From && r.From < reservationPostModel.To) ||
                                    (r.To > reservationPostModel.From && r.To <= reservationPostModel.To)));

            if (existingReservation != null)
            {
                return BadRequest("Reservation already exists in the specified time frame.");
            }

            if (reservationPostModel.From < DateTime.UtcNow || reservationPostModel.To < DateTime.UtcNow)
            {
                return BadRequest("Reservation date must be in the future");
            }


            var tableModel = await _context.Tables.FindAsync(reservationPostModel.TableModelId);

            ReservationModel reservationModel = new ReservationModel()
            {
                From = reservationPostModel.From,
                To = reservationPostModel.To,
                IdentityUserId = reservationPostModel.IdentityUserId,
                TableModelId = reservationPostModel.TableModelId,
            };

            _context.Reservations.Add(reservationModel);

            await _context.SaveChangesAsync();

            return Ok("Reservation created");
        }

        [Authorize(Roles = UserRoles.Worker + "," + UserRoles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPostModel(int id, ReservationPutModel reservationPutModel)
        {
            var reservationFromDb = await _context.Reservations.FindAsync(id);
            if (reservationFromDb == null)
            {
                return NotFound();
            }

            var existingReservation = await _context.Reservations
                        .FirstOrDefaultAsync(r =>
                        r.TableModelId == reservationFromDb.TableModelId &&
                        ((r.From >= reservationPutModel.From && r.From < reservationPutModel.To) ||
                        (r.To > reservationPutModel.From && r.To <= reservationPutModel.To)));

            if (existingReservation != null)
            {
                return BadRequest("Reservation already exists in the specified time frame.");
            }

            if (reservationPutModel.From < DateTime.UtcNow || reservationPutModel.To < DateTime.UtcNow)
            {
                return BadRequest("Reservation date must be in the future");
            }

            reservationFromDb.From = reservationPutModel.From;
            reservationFromDb.To = reservationPutModel.To;

            _context.Entry(reservationFromDb).State = EntityState.Modified;

            await _context.SaveChangesAsync();


            return Ok("Reservation time edited");
        }

        [Authorize(Roles = UserRoles.Worker + "," + UserRoles.Admin)]
        [HttpDelete("/ReservationDelete-Worker/{id}")]
        public async Task<IActionResult> DeleteReservationWorker(int id)
        {
            var reservationModel = await _context.Reservations.FindAsync(id);
            if (reservationModel == null)
            {
                return NotFound();
            }

            _context.Reservations.Remove(reservationModel);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpDelete("/ReservationDelete-User/{id}")]
        public async Task<IActionResult> DeleteReservationUser(int id)
        {
            var reservationModel = await _context.Reservations.FindAsync(id);
            if (reservationModel == null)
            {
                return NotFound();
            }

            if ((reservationModel.From - DateTime.Now).TotalHours < 6)
            {
                return BadRequest("Cannot delete reservation within 6 hours of start time.");
            }

            _context.Reservations.Remove(reservationModel);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpGet("/GetReservations-MobileWeb")]
        public async Task<ActionResult<IEnumerable<ReservationPrivateGetModel>>> GetReservationsMobileWeb()
        {
            var reservations = await _context.Reservations.Where(e => e.From > DateTime.Today && e.To > DateTime.Today).ToListAsync();
            List<ReservationPrivateGetModel> reservationsDto = new List<ReservationPrivateGetModel>();
            foreach (var res in reservations)
            {
                var tmpRes = new ReservationPrivateGetModel()
                {
                    Id = res.Id,
                    From = res.From,
                    To = res.To,
                    TableModelId = res.TableModelId,
                };
                reservationsDto.Add(tmpRes);
            }
            return Ok(reservationsDto);
        }
    }
}
