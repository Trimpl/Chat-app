using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Chat.Models
{
    public class AuthOptions
    {
        public const string ISSUER = "God"; // издатель токена
        public const string AUDIENCE = "Man"; // потребитель токена
        const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
        public const int LIFETIME = 1; // время жизни токена - 1 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
