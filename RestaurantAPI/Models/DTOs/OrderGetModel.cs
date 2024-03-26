using RestaurantAPI.Enums;

namespace RestaurantAPI.Models.DTOs
{
    public class OrderGetModel
    {
        public int Id { get; set; }
        public StatusEnum Status { get; set; }
        public int Price { get; set; }
        public TableGetModel TableModel { get; set; }
        public DishGetModel DishModel { get; set; }
        public UserGetModel IdentityUserModel { get; set; }
    }
}
