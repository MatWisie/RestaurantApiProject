using RestaurantAPI.Enums;

namespace RestaurantAPI.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public StatusEnum Status { get; set; }
        public int Price { get; set; }
        public int TableModelId { get; set; }
        public TableModel TableModel { get; set; }
        public int DishModelId { get; set; }
        public DishModel DishModel { get; set; }
        public string IdentityUserId { get; set; }
        public IdentityUserModel IdentityUserModel { get; set; }
    }
}
