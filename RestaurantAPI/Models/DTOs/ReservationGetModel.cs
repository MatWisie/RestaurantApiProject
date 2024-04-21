namespace RestaurantAPI.Models.DTOs
{
    public class ReservationGetModel
    {
        public int Id { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string IdentityUserId { get; set; }
        public int TableModelId { get; set; }
    }
}
