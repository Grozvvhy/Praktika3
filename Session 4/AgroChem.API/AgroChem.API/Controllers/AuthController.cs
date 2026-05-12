using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web.Http;

namespace AgroChem.API.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        // Модели запросов
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class RegisterRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Role { get; set; }
        }

        public class AuthResponse
        {
            public string Token { get; set; }
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginRequest request)
        {
            try
            {
                // ВРЕМЕННО: ПРОПУСКАЕМ ЛЮБОГО ПОЛЬЗОВАТЕЛЯ ДЛЯ ТЕСТА
                if (!string.IsNullOrWhiteSpace(request.Username))
                {
                    var token = GenerateJwtToken(request.Username, "technologist");
                    return Ok(new AuthResponse { Token = token });
                }

                // Если пользователь не найден
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(RegisterRequest request)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return BadRequest("Логин обязателен");
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("Пароль обязателен");
                }

                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return BadRequest("ФИО обязательно");
                }

                // ВРЕМЕННО: всегда возвращаем успех для теста
                return Ok(new { message = "Регистрация успешна" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private string GenerateJwtToken(string username, string role)
        {
            // Секретный ключ (ДОЛЖЕН СОВПАДАТЬ С КЛЮЧОМ В STARTUP.CS)
            var secretKey = "YourSuperSecretKeyHereAtLeast32CharactersLong!";
            var key = Encoding.UTF8.GetBytes(secretKey);

            var securityKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "AgroChemAPI",
                audience: "AgroChemClient",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}