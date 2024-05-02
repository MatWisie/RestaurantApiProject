namespace RestaurantAPI.Models
{
    public class ReservationPrivateGetModel
    {
        public int Id { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TableModelId { get; set; }
    }
}
