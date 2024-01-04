namespace Authantication.DTOs
{
    public class TokenDTO
    {
        public string Token { get; set; }
        public DateTime ExpireDate { get; set; }
        public Guid RefreshToken { get; set; }
    }
}
