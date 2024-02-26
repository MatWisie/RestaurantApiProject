using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models
{
    public class DishModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Price is required")]
        public double Price { get; set; }
        [Required(ErrorMessage = "Availability is required")]
        public bool Availability { get; set; }

        public ICollection<OrderModel> Orders { get; set; }
    }
}
