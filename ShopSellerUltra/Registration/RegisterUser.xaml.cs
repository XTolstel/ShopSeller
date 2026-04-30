using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AutoSellerUltra.Api;
using static System.Net.WebRequestMethods;

namespace AutoSellerUltra.Registration
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class RegisterUser : Window
    {
        // Важно: укажи ПРАВИЛЬНЫЙ порт твоего Web_Registration
        // Посмотри в выводе при запуске API или в launchSettings.json
        private const string ApiBaseUrl = "https://localhost:5048"; // <-- поменяй при необходимости

        private static readonly HttpClient Http = CreateHttpClient();

        public RegisterUser()
        {
            InitializeComponent();
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();

            // ВАЖНО:
            // На DEV часто ломается из-за self-signed сертификата ASP.NET.
            // Этот код отключает проверку сертификата (ТОЛЬКО для разработки).
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            return new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;
            string dob = BirthDateTextBox.Text;

            /*string login = "EraStiS";
            string email = "oleg.tolstel@gmail.com";
            string password = "123456";
            string dob = "21.06.2005";*/
            // Минимальная проверка на клиенте (не обязательна, но удобно)
            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(dob))
            {
                MessageBox.Show("Fill in all fields.", "Register", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new RegisterRequest
            {
                Login = login.Trim(),
                Email = email.Trim(),
                Password = password,          // пароль обычно не Trim() — пробел может быть частью пароля
                DateOfBirth = dob.Trim()      // формат ожидается dd.MM.yyyy
            };

            try
            {
                string url = $"{ApiBaseUrl}/api/auth/register";

                string json = JsonSerializer.Serialize(dto);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await Http.PostAsync(url, content);
                string responseText = await response.Content.ReadAsStringAsync();//читает ответ сервера как текст 

                ApiResponse? api;
                if (response.IsSuccessStatusCode)
                {
                    api = JsonSerializer.Deserialize<ApiResponse>(
                        responseText,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    MessageBox.Show(api?.Message ?? "Success");
                    // Если сервер вернул нормальный JSON
                    if (api != null)
                    {
                        if (response.IsSuccessStatusCode && api.Success)
                        {
                            MessageBox.Show(api.Message, "Register is complete", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close(); // или очистить поля, как хочешь
                        }
                        else
                        {
                            // 400 BadRequest тоже придёт сюда, и message будет полезным
                            MessageBox.Show(api.Message, "Register failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        return;
                    }
                }
                else
                {
                    // ❗ Если сервер вернул validation error
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        MessageBox.Show(responseText, "Validation error");
                        return;
                    }

                    MessageBox.Show(responseText, "Server error");
                }

                

                // Если сервер вернул не JSON (редко, но бывает)
                MessageBox.Show(
                    $"Server ответил не JSON.\nHTTP: {(int)response.StatusCode}\n\n{responseText}",
                    "Register failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    $"Не удалось подключиться к API.\nПроверь, что Web_Registration запущен и порт правильный.\n\n{ex.Message}",
                    "Network error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show(
                    "Запрос превысил таймаут. Проверь API/порт/HTTPS.",
                    "Timeout",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Неожиданная ошибка:\n{ex}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordWatermark.Visibility =
                string.IsNullOrEmpty(PasswordBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

    }
}
