 using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Google.Protobuf.WellKnownTypes;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using MySql.Data.MySqlClient;
using Write;
using static Write.WriteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.Data;

namespace Write
{

    [ApiController]
    [Route("")]
    public class Verification : ControllerBase
    {

        public static string GetConnectionString()
        {
            // Загружаем конфигурацию из appsettings.json
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            // Получаем секцию ApplicationDb
            var dbConfig = config.GetSection("ApplicationDb");
            var smtpConfig = config.GetSection("EmailSender");
            var portConfig = config.GetSection("Kestrel");

            string server = dbConfig["Server"];
            string database = dbConfig["Database"];
            string user = dbConfig["UserId"];
            string password = dbConfig["Password"];
            password = DecryptFromBase64(password);
            link = portConfig["Endpoints:Https:Url"];
            Login = smtpConfig["Login"];
            Password = smtpConfig["Password"];
            Host = smtpConfig["Host"];
            From = smtpConfig["From"];
            Port = int.Parse(smtpConfig["Port"]);


            // Собираем строку подключения
            return $"Server={server};Database={database};Uid={user};Password={password};SslMode=Required;";
        }

        // Генерирует токен (в виде url-safe строки) и возвращает сам токен; в БД надо сохранить хеш

        //private static string connectionString = "Server=localhost;Port=3306;Database=Webka;Uid=root;Password=1894350;SslMode=None;";
        private static string connectionString;

        public static string Login;

        public static string Password;

        public static string Host;

        public static string From;

        static int Port;

        public static string link;


        [HttpGet("/confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string login, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(token))
                return BadRequest("Wrong parameters of link.");

            string tokenHash = ComputeSha256Hash(token);

            using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();

            // 1. Проверяем запись в таблице EmailConfirmations
            string selectSql = @"SELECT login, Expiration_date, IsEmailConfirmed 
                             FROM Users 
                             WHERE login = @login AND EmailConfirmationToken = @tokenHash AND IsEmailConfirmed = 0
                             LIMIT 1;";

            using var cmd = new MySqlCommand(selectSql, conn);
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@tokenHash", tokenHash);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.Read())
                return BadRequest("Wrong or used token.");

            DateTime expiresAt = reader.GetDateTime("Expiration_date");
            bool used = reader.GetBoolean("IsEmailConfirmed");
            // Читаем login из базы как строку
            string confirmationLogin = reader.GetString("login");


            reader.Close();

            if (expiresAt < DateTime.UtcNow)
                return BadRequest("Token is expired.");

            // 2. "Сжигаем" токен — обнуляем хеш токена
            string updateConfirmationSql = @"UPDATE Users 
                                 SET EmailConfirmationToken = NULL 
                                 WHERE login = @login;";

            using var updateCmd = new MySqlCommand(updateConfirmationSql, conn);
            updateCmd.Parameters.AddWithValue("@login", confirmationLogin);
            await updateCmd.ExecuteNonQueryAsync();


            // 3. Обновляем статус пользователя
            string updateUserSql = @"UPDATE Users SET IsEmailConfirmed = 1 WHERE login = @login;";
            using var updateUserCmd = new MySqlCommand(updateUserSql, conn);
            updateUserCmd.Parameters.AddWithValue("@login", login);
            await updateUserCmd.ExecuteNonQueryAsync();


            // 3. Обновляем время истечения
            string updateTimeSql = @"UPDATE Users SET Expiration_date = NULL WHERE login = @login;";
            using var updateTimeCmd = new MySqlCommand(updateTimeSql, conn);
            updateTimeCmd.Parameters.AddWithValue("@login", login);
            await updateTimeCmd.ExecuteNonQueryAsync();
            return Content(@"
            <html>
              <head><title>Email confirmed</title></head>
              <body style='font-family:Arial;'>
                <h2>Nice, Email is confirmed; !</h2>
              </body>
            </html>", "text/html");
        }


        // Метод поиска пользователя по токену
        public static string FindUserByToken(string token)
        {
            string confirmationLink = null;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT login, email FROM Users WHERE EmailConfirmationToken=@tokenHash LIMIT 1";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@tokenHash", ComputeSha256Hash(token));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var user = new User
                            {
                                Login = reader.GetString("login"),
                                Email = reader.GetString("email")
                            };

                            confirmationLink = BuildConfirmationLink(
                                link,
                                user.Login,
                                user.Email, // используем Email как идентификатор
                                token
                            );


                            return confirmationLink;
                        }
                    }
                }
            }
            return null; // Пользователь с таким токеном не найден
        }
        // Создание ссылки подтверждения
        public static string BuildConfirmationLink(string baseUrl, string Login, string email, string token)
        {
            var encodedToken = HttpUtility.UrlEncode(token);
            //return $"{baseUrl.TrimEnd('/')}/confirm-email?userLogin={Login}&userEmail={email}&token={encodedToken}";
            //return $"{baseUrl.TrimEnd('/')}/confirm-email?login={Login}&token={encodedToken}";
            return $"{baseUrl.TrimEnd('/')}/confirm-email?login={Login}&token={encodedToken}";

        }

        public static string GenerateToken()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(32);
            return WebEncoders.Base64UrlEncode(bytes);
        }

        public static string ComputeSha256Hash(string value)
        {
            {
                using var sha = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(value);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToHexString(hash);
            }
        }

        // Отправка HTML-письма с кнопкой
        public static async Task SendConfirmationEmail(string recipientEmail, string token)
        {
            connectionString = GetConnectionString();
            try
            {
                string link = FindUserByToken(token);
                Console.WriteLine("Your link ", link);
                // HTML с кнопкой подтверждения
                var htmlBody = $@"
            <html>
              <body style='font-family: Arial, sans-serif;'>
                <p>Hello,</p>
                <p>Please, confirm your email, by clicking the button below:</p>
                <p>
                  <a href='{link}' style='display:inline-block;padding:12px 20px;border-radius:6px;text-decoration:none;
                     font-weight:600;border:1px solid #1a73e8;background-color:#1a73e8;color:white;'>
                    Confirm email
                  </a>
                </p>
                <p>If the button does not work, open the link manually.:</p>
                <p><a href='{link}'>{link}</a></p>
                <p>Sincerely,<br/>EraStiS</p>
              </body>
            </html>";

                // Формируем письмо
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Opta Events", Login));
                message.To.Add(MailboxAddress.Parse(recipientEmail));
                message.Subject = "Confirm email";
                message.Body = new TextPart("html") { Text = htmlBody }; // HTML вместо plain

                // Отправка через SMTP
                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(Host, Port, MailKit.Security.SecureSocketOptions.StartTls);
                    smtp.Authenticate(Login, Password);
                    smtp.Timeout = 20000;
                    smtp.Send(message);
                    smtp.Disconnect(true);
                }

                Console.WriteLine("Confirmation email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending confirmation email: " + ex.Message);
            }
        }

    }
}