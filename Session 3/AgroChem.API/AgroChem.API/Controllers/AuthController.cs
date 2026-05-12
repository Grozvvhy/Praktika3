using System.Web.Http;
using AgroChem.API.Services;

namespace AgroChem.API.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly AuthService _auth = new AuthService();

        public class LoginRequest
        {
            public string Login { get; set; }
            public string Password { get; set; }
        }

        [HttpPost, Route("login")]
        [AllowAnonymous]
        public IHttpActionResult Login([FromBody] LoginRequest request)
        {
            var token = _auth.Authenticate(request.Login, request.Password);
            if (token == null)
                return Unauthorized();
            return Ok(new { success = true, data = new { token } });
        }
    }
}