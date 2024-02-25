using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;

namespace RestaurantAPI
{
    public class RestaurantDbContext : IdentityDbContext<IdentityUserModel>
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
    : base(options)
        {
        }
    }
}
