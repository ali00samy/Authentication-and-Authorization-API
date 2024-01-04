using Microsoft.AspNetCore.Identity;

namespace Authantication.Data.Models
{
    public class User : IdentityUser
    {
        public string Departmnet { get; set; }
        public string Nationality { get; set; }
        public string Token { get; set; }
        public Guid RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryDate { get; set; }
    }
}
