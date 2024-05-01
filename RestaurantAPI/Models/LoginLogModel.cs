using System.Net;

namespace RestaurantAPI.Models
{
    public class LoginLogModel
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public bool Success { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public HttpStatusCode StatusCode { get; set; }
        public string? IdentityUserId { get; set; }
        public virtual IdentityUserModel? IdentityUserModel { get; set; }
    }
}
