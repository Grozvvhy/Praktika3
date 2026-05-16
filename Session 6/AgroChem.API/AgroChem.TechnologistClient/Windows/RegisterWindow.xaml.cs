using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;
using AgroChem.TechnologistClient.Helpers;
using AgroChem.TechnologistClient.Services;

namespace AgroChem.TechnologistClient.Windows
{
    public partial class RegisterWindow : Window
    {
        private ApiService _api;
        private string _currentCaptchaCode;

        public RegisterWindow(ApiService api)
        {
            InitializeComponent();
            _api = api;

            btnRegister.Click += Register_Click;
            btnCancel.Click += Cancel_Click;
            btnRefreshCaptcha.Click += RefreshCaptcha_Click;

            RefreshCaptcha();
        }

        private void RefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            RefreshCaptcha();
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnRegister.IsEnabled = false;
                txtError.Text = "Проверка капчи...";

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
                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    txtError.Text = "Введите ФИО";
                    return;
                }

                txtError.Text = "Отправка запроса на сервер...";

                // Показываем, какие данные отправляем
                var logMessage = $"Отправка: username={txtUsername.Text}, email={txtEmail.Text}, phone={txtPhone.Text}";
                System.Diagnostics.Debug.WriteLine(logMessage);

                var success = await _api.RegisterAsync(
                    txtUsername.Text,
                    txtPassword.Password,
                    txtFullName.Text,
                    txtEmail.Text,
                    txtPhone.Text,
                    "technologist");

                if (success)
                {
                    MessageBox.Show("Регистрация успешна! Теперь войдите.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else
                {
                    // Получаем более детальную ошибку
                    txtError.Text = "Сервер вернул ошибку. Пользователь может существовать или данные неверны.";

                    // Дополнительная диагностика
                    var result = MessageBox.Show("Показать детали ошибки для диагностики?", "Ошибка регистрации",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // Пробуем получить полный ответ от сервера
                            var client = new HttpClient();
                            var content = new StringContent(
                                $"{{\"username\":\"{txtUsername.Text}\",\"password\":\"{txtPassword.Password}\",\"fullName\":\"{txtFullName.Text}\",\"email\":\"{txtEmail.Text}\",\"phone\":\"{txtPhone.Text}\",\"role\":\"technologist\"}}",
                                System.Text.Encoding.UTF8, "application/json");
                            var response = await client.PostAsync("https://localhost:44308/api/auth/register", content);
                            var errorBody = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Статус: {response.StatusCode}\nТело ответа: {errorBody}", "Детали ошибки",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при диагностике: {ex.Message}", "Диагностика",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                txtError.Text = $"Нет связи с сервером: {ex.Message}";
                MessageBox.Show($"API не отвечает. Убедитесь, что API запущен.\n\n{ex.Message}", "Ошибка подключения",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                txtError.Text = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Детали: {ex.ToString()}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnRegister.IsEnabled = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
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