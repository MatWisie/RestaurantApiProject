namespace RestaurantAPI.Models
{
    public class ReservationModel
    {
        public int Id { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string IdentityUserId { get; set; }
        public virtual IdentityUserModel IdentityUserModel { get; set; }
        public int TableModelId { get; set; }
        public virtual TableModel TableModel { get; set; }
    }
}
