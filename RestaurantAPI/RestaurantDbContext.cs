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
        public DbSet<DishModel> Dishes { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<ReservationModel> Reservations { get; set; }
        public DbSet<TableModel> Tables { get; set; }
    }
}
