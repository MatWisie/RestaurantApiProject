using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models
{
    public class TableModel
    {
        public int Id { get; set; }
        public bool IsAvailable { get; set; } = true;
        [Required]
        public int NumberOfSeats { get; set; }

        public ICollection<OrderModel> OrderModels { get; set; }
        public ICollection<ReservationModel> Reservations { get; set; }
    }
}
