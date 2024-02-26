namespace RestaurantAPI.Models
{
    public class ReservationModel
    {
        public int Id { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int IdentityUserId { get; set; }
        public IdentityUserModel IdentityUserModel { get; set; }
        public int TableModelId { get; set; }
        public TableModel TableModel { get; set; }
    }
}
