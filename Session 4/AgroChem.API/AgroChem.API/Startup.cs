using System.Configuration;
using System.Text;
using System.Web.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;

[assembly: OwinStartup(typeof(AgroChem.API.Startup))]

namespace AgroChem.API
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Настройка Web API
            var config = new HttpConfiguration();

            // Маршруты Web API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Настройка JWT аутентификации
            ConfigureJwtAuthentication(app);

            // Включение Web API
            app.UseWebApi(config);
        }

        private void ConfigureJwtAuthentication(IAppBuilder app)
        {
            // Секретный ключ (должен совпадать с ключом в контроллере)
            var secretKey = "YourSuperSecretKeyHereAtLeast32CharactersLong!";
            var key = Encoding.UTF8.GetBytes(secretKey);

            // Параметры валидации токена
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "AgroChemAPI",
                ValidateAudience = true,
                ValidAudience = "AgroChemClient",
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = System.TimeSpan.Zero
            };

            // Настройка JWT аутентификации
            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                TokenValidationParameters = validationParameters
            });
        }
    }
}