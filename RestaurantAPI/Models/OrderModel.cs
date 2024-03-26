using RestaurantAPI.Enums;

namespace RestaurantAPI.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public StatusEnum Status { get; set; } = StatusEnum.Working;
        public int Price { get; set; }
        public int TableModelId { get; set; }
        public virtual TableModel TableModel { get; set; }
        public List<DishModel> DishModels { get; set; }
        public string IdentityUserId { get; set; }
        public virtual IdentityUserModel IdentityUserModel { get; set; }
    }
}
