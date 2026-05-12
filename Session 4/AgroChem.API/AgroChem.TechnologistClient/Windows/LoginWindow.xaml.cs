using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using AgroChem.TechnologistClient.Helpers;
using AgroChem.TechnologistClient.Services;

namespace AgroChem.TechnologistClient.Windows
{
    public partial class LoginWindow : Window
    {
        private ApiService _api;
        private string _currentCaptchaCode;

        public LoginWindow()
        {
            InitializeComponent();
            // ВАЖНО: используем HTTPS с портом 44308
            _api = new ApiService("https://localhost:44308");

            btnLogin.Click += Login_Click;
            btnRegister.Click += Register_Click;
            btnRefreshCaptcha.Click += RefreshCaptcha_Click;

            RefreshCaptcha();
        }

        private void RefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            RefreshCaptcha();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnLogin.IsEnabled = false;

                if (txtCaptcha.Text != _currentCaptchaCode)
                {
                    txtError.Text = "Неверная капча";
                    RefreshCaptcha();
                    return;
                }

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

                txtError.Text = "Подключение к серверу...";

                var token = await _api.LoginAsync(txtUsername.Text, txtPassword.Password);
                _api.SetToken(token);

                var mainWindow = new MainWindow(_api, txtUsername.Text);
                mainWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                txtError.Text = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Детали ошибки:\n{ex.Message}", "Ошибка входа",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnLogin.IsEnabled = true;
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var regWindow = new RegisterWindow(_api);
            regWindow.Owner = this;
            regWindow.ShowDialog();
        }

        private void RefreshCaptcha()
        {
            try
            {
                var (imgBytes, code) = CaptchaHelper.GenerateCaptcha();
                using (var ms = new MemoryStream(imgBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imgCaptcha.Source = bitmap;
                }
                _currentCaptchaCode = code;
                txtError.Text = "";
            }
            catch (Exception ex)
            {
                txtError.Text = $"Ошибка капчи: {ex.Message}";
            }
        }
    }
}