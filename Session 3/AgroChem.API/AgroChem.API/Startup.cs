using System.Configuration;
using System.Text;
using System.Web.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;   // вот это
using Microsoft.Owin.Security.Jwt;
using Owin;

[assembly: OwinStartup(typeof(AgroChem.API.Startup))]

namespace AgroChem.API
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // ... без изменений
        }
    }
}