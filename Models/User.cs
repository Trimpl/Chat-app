using Microsoft.AspNetCore.Identity;

namespace Chat.Models
{
    public class User : IdentityUser
    {
        public string Img { get; set; }
    }
}