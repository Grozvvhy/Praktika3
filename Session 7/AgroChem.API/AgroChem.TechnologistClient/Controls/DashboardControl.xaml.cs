using AgroChem.TechnologistClient.Services;
using System.Windows.Controls;

namespace AgroChem.TechnologistClient.Controls
{
    public partial class DashboardControl : UserControl
    {
        public DashboardControl(ApiService api)
        {
            InitializeComponent();
        }
    }
}