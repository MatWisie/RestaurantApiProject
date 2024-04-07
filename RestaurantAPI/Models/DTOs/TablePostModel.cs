using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models.DTOs
{
    public class TablePostModel
    {
        public bool IsAvailable { get; set; } = true;
        [Required]
        public int NumberOfSeats { get; set; }
        [Required]
        public int GridRow { get; set; }
        [Required]
        public int GridColumn { get; set; }
    }
}
