using System;
using System.Windows;
using AgroChem.OperatorClient.Services;

namespace AgroChem.OperatorClient
{
    public partial class LoginWindow : Window
    {
        private ApiService _api;

        public LoginWindow()
        {
            InitializeComponent();
            _api = new ApiService("https://localhost:44308");
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnLogin.IsEnabled = false;
                txtError.Text = "";

                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    txtError.Text = "Введите логин";
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    txtError.Text = "Введите пароль";
                    return;
                }

                var (success, role, fullName) = await _api.LoginAsync(txtUsername.Text, txtPassword.Password);
                if (success && role == "operator")
                {
                    var mainWindow = new ActiveBatchesWindow(_api, fullName);
                    mainWindow.Show();
                    Close();
                }
                else if (success && role != "operator")
                {
                    txtError.Text = "У вас нет прав аппаратчика";
                }
                else
                {
                    txtError.Text = "Неверный логин или пароль";
                }
            }
            catch (Exception ex)
            {
                txtError.Text = "Ошибка подключения к серверу";
                MessageBox.Show($"Детали: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnLogin.IsEnabled = true;
            }
        }
    }
}