using System.Windows;

namespace AgroChem.Technologist
{
    public partial class App : Application
    {
        public static string Token { get; set; }
        public static string ApiBaseUrl { get; set; } = "http://localhost:55555/api/";
    }
}