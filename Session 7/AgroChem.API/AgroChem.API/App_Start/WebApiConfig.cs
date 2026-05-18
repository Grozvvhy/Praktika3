using System.Web.Http;

namespace AgroChem.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Включаем атрибутную маршрутизацию
            config.MapHttpAttributeRoutes();

            // Стандартный маршрут
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Только JSON, без XML
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
                Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}