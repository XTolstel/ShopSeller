using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
//using System.Data.SqlClient;
//using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Write;
using static Mysqlx.Expect.Open.Types.Condition.Types;
using static Write.Verification;
using static Write.WriteDB;
namespace Write
{

    public static class WriteDB
    {
        public class User
        {
            public int Id { get; set; }

            public string Login { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }

            public String DateOfBirth { get; set; }


            public bool isemailcon { get; set; }

            public DateTime expiration { get; set; }

        }
        


        // Симметричный ключ и IV для AES (16 байт каждый)
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("1234567890123456");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("6543210987654321");

        //static string connectionString = "Server=localhost;Port=3306;Database=Webka;Uid=root;Password=1894350;SslMode=Required;";

        static string connectionString;

        public static string DecryptFromBase64(string base64Cipher)
        {
            Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] cipherBytes = Convert.FromBase64String(base64Cipher);
            byte[] decrypted = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(decrypted);
        }
        public static string GetConnectionString()
        {
            // Загружаем конфигурацию из appsettings.json
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            // Получаем секцию ApplicationDb
            var dbConfig = config.GetSection("ApplicationDb");

            string server = dbConfig["Server"];
            string database = dbConfig["Database"];
            string user = dbConfig["UserId"];
            string password = dbConfig["Password"];
            password = DecryptFromBase64(password);
            // Собираем строку подключения
            return $"Server={server};Database={database};Uid={user};Password={password};SslMode=Required;";
        }

        public static string UpdatePasswordByEmail(string email, string newPassword)
        {
            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                string sql = @"UPDATE Users SET Password = @password WHERE Email = @email;";
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@password", newPassword);
                cmd.Parameters.AddWithValue("@email", email);

                int affectedRows = cmd.ExecuteNonQuery();

                if (affectedRows > 0)
                    return "Password successfully updated.";
                else
                    return "User with this email not found.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public static void Start()
        {
            // Запускаем фоновую задачу
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        connectionString = GetConnectionString();

                        using (var conn = new MySqlConnection(connectionString))
                        {
                            await conn.OpenAsync();

                            string sql = @"
                                DELETE FROM Users 
                                WHERE IsEmailConfirmed = 0 
                                AND Expiration_date IS NOT NULL 
                                AND Expiration_date < @now";

                            using (var cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@now", DateTime.Now);

                                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                                if (rowsAffected > 0)
                                    Console.WriteLine($"Deleted {rowsAffected} expired users at {DateTime.Now}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in ExpiredUserCleaner: {ex.Message}");
                    }

                    // Ждём 20 секунд перед следующей проверкой
                    await Task.Delay(20000);
                }
            });
        }

        public static void Start_Promo()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        connectionString = GetConnectionString();

                        using (var conn = new MySqlConnection(connectionString))
                        {
                            await conn.OpenAsync();

                            string sql = @"
                                DELETE FROM Promocodes
                                WHERE expiration_date IS NOT NULL
                                AND expiration_date < @now";

                            using (var cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@now", DateTime.Now);

                                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                                if (rowsAffected > 0)
                                    Console.WriteLine($"Deleted {rowsAffected} expired promocodes at {DateTime.Now}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in ExpiredPromocodeCleaner: {ex.Message}");
                    }

                    await Task.Delay(20000);
                }
            });
        }

        public static User GetUserByEmail(string Email)
        {
            if (string.IsNullOrWhiteSpace(Email))
                return null;

            Email = Email.Trim();
            connectionString = GetConnectionString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                        SELECT id, login, email, password, datebirth
                        FROM Users
                        WHERE lower(email) = lower(@email)
                        LIMIT 1;
                        ";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@email", Email);

                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                                return null;

                            return new User
                            {
                                Id = reader.GetInt32("id"),
                                Login = reader.GetString("login"),
                                Email = reader.GetString("email"),
                                Password = reader.GetString("password"),
                                DateOfBirth = reader.GetString("datebirth")
                            };
                        }
                    }
                }
                catch
                {
                    return null; // или логировать
                }
            }
        }


        public static string Login(String Email, string Password)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                return "Email and password are required.";
            Email = Email.Trim();
            connectionString = GetConnectionString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    Console.WriteLine("Connection with database is stable");

                    string query = @"
                        SELECT Password,login,datebirth,id,IsEmailConfirmed
                        FROM Users
                        WHERE lower(email) = lower(@email)
                        LIMIT 1;
                        ";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {

                        string pass = Password; // сделал строкой
                        string email = Email;

                        command.Parameters.AddWithValue("@email", email);

                        using var reader = command.ExecuteReader();
                        if (!reader.Read())
                            return "User not found.";

                        int isEmailConfirmed = Convert.ToInt32(reader["IsEmailConfirmed"]);
                        if (isEmailConfirmed != 1)
                            return "Email is not verified for ShopSeller";

                        var dbPassword = reader.GetString("Password");

                        if (dbPassword == pass)
                            return "Login successful.";
                        else
                            return "Invalid email or password.";
                    }
                }
                catch (Exception ex)
                {
                    // В проде лучше логировать, но пока вернём текст
                    return $"Database error: {ex.Message}";
                }

            }
        }
        public static string Write_toDB(User user)
        {

            connectionString = GetConnectionString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    Console.WriteLine("Connection with database is stable");

                    string query = "INSERT INTO Users (login, password, email, datebirth, EmailConfirmationToken, Expiration_date, IsEmailConfirmed)" +
                        " VALUES (@login, @pass, @email, @date, @emailtoken, @exp_time, @emailcon)";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        string login = user.Login;
                        string pass = user.Password; // сделал строкой
                        string email = user.Email;
                        String dateOnly = user.DateOfBirth;

                        
                        string token = Verification.GenerateToken();
                        string token_hash = Verification.ComputeSha256Hash(token);

                        DateTime expireTime = DateTime.Now.AddMinutes(3); // текущее время + 3 минуты
                        //string token = Guid.NewGuid().ToString();
                        command.Parameters.AddWithValue("@login", login);
                        command.Parameters.AddWithValue("@pass", pass);
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@date", dateOnly);
                        command.Parameters.AddWithValue("@emailtoken", token_hash);
                        command.Parameters.AddWithValue("@exp_time", expireTime);
                        command.Parameters.AddWithValue("@emailcon", false);


                        int rowsAffected = command.ExecuteNonQuery();
                        Thread.Sleep(1000); // пауза 2000 мс (2 секунды)
                        Verification.SendConfirmationEmail(email, token);
                        //await Verification.SendConfirmationEmail(email, token, config["AppSettings:BaseUrl"]);

                        if (rowsAffected > 0)
                        {
                            return "Sending is complete";
                        }
                        else
                        {
                            return "Sending is failure";
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    // Код ошибки 1062 = Duplicate entry (уникальность нарушена)
                    if (ex.Number == 1062)
                    {
                        return "This email is already registered.";
                    }
                    else return "Sending error";
                }

            }
        }

    }
}
