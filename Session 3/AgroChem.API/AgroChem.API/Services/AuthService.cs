using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AgroChem.Data;
using AgroChem.Data.Models;

namespace AgroChem.API.Services
{
    public class AuthService
    {
        public string Authenticate(string login, string password)
        {
            using (var db = new AppDbContext())
            {
                var user = db.Users.Include("Role")
                    .FirstOrDefault(u => u.Login == login && u.PasswordHash == Hash(password));
                if (user == null) return null;

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Login),
                    new Claim(ClaimTypes.Role, user.Role.Name)
                };

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(System.Configuration.ConfigurationManager.AppSettings["JwtKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: System.Configuration.ConfigurationManager.AppSettings["JwtIssuer"],
                    audience: System.Configuration.ConfigurationManager.AppSettings["JwtAudience"],
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }

        private string Hash(string password)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}