using api_cleany_app.src.Models;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace api_cleany_app.src.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration configuration;

        public JwtHelper(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GenerateJwtToken(User user)
        {
            string roleName = user.RoleId switch
            {
                1 => "Cleaners",
                2 => "Users",
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Username",user.Username),
                    new Claim("Email",user.Email),
                    new Claim("Password",user.Password),
                    new Claim(ClaimTypes.Role, roleName)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}