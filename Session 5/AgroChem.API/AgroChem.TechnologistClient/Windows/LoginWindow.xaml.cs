using System;
using System.IO;
using System.Net.Http;
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
                txtError.Text = "";

                // Проверка капчи
                if (string.IsNullOrWhiteSpace(txtCaptcha.Text) || txtCaptcha.Text != _currentCaptchaCode)
                {
                    txtError.Text = "Неверная капча";
                    RefreshCaptcha();
                    txtCaptcha.Text = "";
                    return;
                }

                // Проверка логина
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    txtError.Text = "Введите логин";
                    return;
                }

                // Проверка пароля
                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    txtError.Text = "Введите пароль";
                    return;
                }

                txtError.Text = "Подключение к серверу...";

                // Попытка входа (без токена)
                var success = await _api.LoginAsync(txtUsername.Text, txtPassword.Password);

                if (success)
                {
                    // Успешный вход - открываем главное окно
                    var mainWindow = new MainWindow(_api, txtUsername.Text);
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    txtError.Text = "Неверный логин или пароль";
                }
            }
            catch (HttpRequestException ex)
            {
                txtError.Text = "Ошибка подключения к серверу. Убедитесь, что API запущен.";
                MessageBox.Show($"Не удалось подключиться к API.\n\nПроверьте, что API запущен по адресу: https://localhost:44308\n\nДетали: {ex.Message}",
                    "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
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
            try
            {
                var regWindow = new RegisterWindow(_api);
                regWindow.Owner = this;
                regWindow.ShowDialog();

                // После закрытия окна регистрации обновляем капчу
                RefreshCaptcha();
                txtCaptcha.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна регистрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                txtError.Text = $"Ошибка генерации капчи: {ex.Message}";
            }
        }
    }
}