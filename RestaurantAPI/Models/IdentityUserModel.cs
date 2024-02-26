using Microsoft.AspNetCore.Identity;

namespace RestaurantAPI.Models
{
    public class IdentityUserModel : IdentityUser
    {
        public bool FirstTimeLoggedIn { get; set; }
        public int Age { get; set; }

        public ICollection<OrderModel> OrderModels { get; set; }
        public ICollection<ReservationModel> Reservations { get; set; }

    }
}
