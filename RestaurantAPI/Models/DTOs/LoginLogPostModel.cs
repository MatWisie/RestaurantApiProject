using System.Net;

namespace RestaurantAPI.Models.DTOs
{
    public class LoginLogPostModel
    {
        public string IpAddress { get; set; }
        public bool Success { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string IdentityUserId { get; set; }
    }
}
