using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using AutoSellerUltra.Api;
using static Write.WriteDBUser;
namespace AutoSellerUltra.Login
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class LoginUser : Window
    {
        private const string ApiBaseUrl = "https://localhost:5048"; // <-- твой порт из консоли Web_Registration

        private static readonly HttpClient Http = CreateHttpClient();

        public LoginUser()
        {
            InitializeComponent();
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                // Только для разработки (dev-сертификат)
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            return new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            //var email = EmailTextBox.Text?.Trim() ?? "";
            //var password = PasswordBox.Password ?? "";

            string email = "oleg.tolstel@gmail.com";
            string password = "123456";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите email и пароль.", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new LoginRequest
            {
                Email = email,
                Password = password
            };

            try
            {
                var url = $"{ApiBaseUrl}/api/auth/login";
                var json = JsonSerializer.Serialize(dto);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await Http.PostAsync(url, content);
                var responseText = await response.Content.ReadAsStringAsync();

                // Пытаемся прочитать как ApiResponse
                ApiResponse? api = null;
                if (response.IsSuccessStatusCode)
                {
                    api = JsonSerializer.Deserialize<ApiResponse>(
                        responseText,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (api != null)
                    {
                        if (response.IsSuccessStatusCode && api?.Success == true)
                        {
                            UserDto User = api.user;
                            (User.balance, User.spendbalance) = LoadBalance(User.Id);
                            UserSession.SetUser(User);
                            DialogResult = true;   // важно
                            WriteAutoUserIfNotExists(User);

                            //MessageBox.Show(api.Message == "" ? "Login successful." : api.Message,
                            //"Login", MessageBoxButton.OK, MessageBoxImage.Information);
                            MessageBox.Show(
                                $"Login successful.\n\n" +
                                $"ID: {User.Id}\n" +
                                $"Login: {User.Login}\n" +
                                $"Email: {User.Email}\n" +
                                $"Date of birth: {User.DateOfBirth}",
                                "Login",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            
                            // пока просто закрываем окно
                            Close();
                            return;
                        }
                        else
                        {
                            // 400 BadRequest тоже придёт сюда, и message будет полезным
                            MessageBox.Show(api.Message, "Login failed", MessageBoxButton.OK, MessageBoxImage.Error);
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

                

                // Неверный пароль/пользователь (401) или другие ошибки (400/500)
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    MessageBox.Show(api?.Message ?? "Wrong email or password.",
                        "Login failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Если API вернул не ApiResponse (например validation JSON или текст)
                MessageBox.Show(api?.Message ?? responseText,
                    "Login failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к API:\n{ex.Message}",
                    "Network error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
