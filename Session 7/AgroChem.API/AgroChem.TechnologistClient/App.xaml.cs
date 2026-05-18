using System.Windows;
using AgroChem.TechnologistClient.Windows;

namespace AgroChem.TechnologistClient
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var login = new LoginWindow();
            login.Show();
        }
    }
}