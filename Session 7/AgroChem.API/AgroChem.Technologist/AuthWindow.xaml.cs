using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using AgroChem.Technologist.Helpers;

namespace AgroChem.Technologist
{
    public partial class AuthWindow : Window
    {
        private bool _isRegisterMode = false;
        private readonly HttpClient _client;

        public AuthWindow()
        {
            InitializeComponent();
            _client = new HttpClient { BaseAddress = new Uri(App.ApiBaseUrl) };
            GenerateCaptcha();
            LoadRoles();
        }

        private void GenerateCaptcha()
        {
            CaptchaGenerator.Generate();
            CaptchaImage.Source = CaptchaGenerator.Image;
        }

        private async void LoadRoles()
        {
            try
            {
                var resp = await _client.GetAsync("api/roles");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var apiResp = JsonConvert.DeserializeObject<ApiResponse<List<RoleDto>>>(json);
                    RoleCombo.ItemsSource = apiResp.Data;
                }
            }
            catch { }
        }

        private void OnRefreshCaptcha(object sender, RoutedEventArgs e) => GenerateCaptcha();

        private void ToggleMode(bool isRegister)
        {
            _isRegisterMode = isRegister;
            RegPanel.Visibility = isRegister ? Visibility.Visible : Visibility.Collapsed;
            LoginBtn.Content = isRegister ? "Зарегистрироваться" : "Войти";
            RegisterBtn.Content = isRegister ? "К входу" : "Регистрация";
            ErrorText.Text = "";
        }

        private void OnLoginClick(object sender, RoutedEventArgs e)
        {
            if (CaptchaBox.Text != CaptchaGenerator.Code)
            {
                ErrorText.Text = "Неверная капча";
                GenerateCaptcha();
                return;
            }
            if (!_isRegisterMode) Authenticate();
            else RegisterUser();
        }

        private void OnRegisterClick(object sender, RoutedEventArgs e) => ToggleMode(!_isRegisterMode);

        private async void Authenticate()
        {
            var data = new { Login = LoginBox.Text, Password = PasswordBox.Password };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("api/auth/login", content);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(json);
                App.Token = (string)result.data.token;
                new MainWindow().Show();
                Close();
            }
            else
            {
                ErrorText.Text = "Неверный логин или пароль";
                GenerateCaptcha();
            }
        }

        private async void RegisterUser()
        {
            if (RoleCombo.SelectedValue == null)
            {
                ErrorText.Text = "Выберите роль";
                return;
            }
            var data = new
            {
                FullName = FullNameBox.Text,
                Login = LoginBox.Text,
                Password = PasswordBox.Password,
                RoleId = (int)RoleCombo.SelectedValue
            };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var resp = await _client.PostAsync("api/users", content);
            if (resp.IsSuccessStatusCode)
            {
                MessageBox.Show("Регистрация успешна. Войдите.", "Успех");
                ToggleMode(false);
            }
            else
            {
                ErrorText.Text = "Ошибка регистрации";
                GenerateCaptcha();
            }
        }
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}