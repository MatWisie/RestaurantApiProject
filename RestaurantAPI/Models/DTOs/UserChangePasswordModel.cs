namespace RestaurantAPI.Models.DTOs
{
    public class UserChangePasswordModel
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
    }
}
