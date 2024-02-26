namespace RestaurantAPI.Models.DTOs
{
    public class DishGetModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public bool Availability { get; set; }
    }
}
