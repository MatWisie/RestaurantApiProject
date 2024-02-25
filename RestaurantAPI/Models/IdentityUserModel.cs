using Microsoft.AspNetCore.Identity;

namespace RestaurantAPI.Models
{
    public class IdentityUserModel : IdentityUser
    {
        public bool FirstTimeLoggedIn { get; set; }

    }
}
