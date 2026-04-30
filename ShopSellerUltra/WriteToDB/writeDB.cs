using System;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using AutoSellerUltra.AutoWindow;
using Microsoft.Extensions.Configuration;
//using System.Data.SqlClient;
//using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using static Mysqlx.Expect.Open.Types.Condition.Types;
namespace Write
{
    public class Auto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }

        public int Price { get; set; }
        public int Quantity { get; set; }

        public byte[]? ImageBytes { get; set; }

    }
    public class WriteDB
    {
      


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

        public static List<Auto> LoadCarsFromDb()
        {
            var cars = new List<Auto>();

            connectionString = GetConnectionString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                       SELECT id, name, price, category, quantity, image
                       FROM Auto;";

                    using var cmd = new MySqlCommand(query, connection);
                    using var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        byte[]? imgBytes = null;

                        // image может быть NULL в БД
                        int imageOrdinal = reader.GetOrdinal("image");
                        if (!reader.IsDBNull(imageOrdinal))
                        {
                            imgBytes = (byte[])reader.GetValue(imageOrdinal);
                        }

                        cars.Add(new Auto
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name"),
                            Price = reader.GetInt32("price"),
                            Category = reader.GetString("category"),
                            Quantity = reader.GetInt32("quantity"),
                            ImageBytes = imgBytes
                        });
                    }
                }
                catch (MySqlException ex)
                {
                    //Console.WriteLine("Ошибка: " + ex.Message);
                    MessageBox.Show(
                    ex.Message,
                    "Database error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                }
            }
                    return cars;
        }


        public static string Buy_Auto(ObservableCollection<Auto> Cart_cars, Dictionary<int, int> CartCounts)
        {
            connectionString = GetConnectionString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();
                        try
                        {
                            
                            string query = @"
                                UPDATE Auto
                                SET Quantity = Quantity - @Quantity
                                WHERE Id = @Id";

                            using (MySqlCommand command = new MySqlCommand(query, connection,transaction))
                            {


                                command.Parameters.AddWithValue("@Id", MySqlDbType.Int32);
                                command.Parameters.AddWithValue("@Quantity", MySqlDbType.Int32);

                            foreach (var auto in Cart_cars)
                            {
                                command.Parameters["@Id"].Value = auto.Id;

                                // если в корзине каждая машина = 1
                                //command.Parameters["@Quantity"].Value = 1;

                                // ❗ если есть auto.Quantity (сколько купили)
                                command.Parameters["@Quantity"].Value = CartCounts[auto.Id];

                                command.ExecuteNonQuery();
                            }
                            
                            }
                        // ✅ если хочешь удалить нулевые в этой же транзакции — делай тут:
                        using (var deleteCmd = new MySqlCommand("DELETE FROM Auto WHERE Quantity <= 0;", connection, transaction))
                        {
                            deleteCmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        //DeleteZeroAuto();
                    }
                        catch (MySqlException ex)
                        {
                        transaction.Rollback();
                        MessageBox.Show(
                                ex.Message,
                                "Database error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );

                            return "Updating quantity failed";
                        }

                    
            }
            
                return null;
        }
        public static string DeleteZeroAuto()
        {
            connectionString = GetConnectionString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();
                try
                    {

                    string query = @"
                                DELETE FROM Auto
                                WHERE quantity <= 0 ";

                    using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
                        {
                                command.ExecuteNonQuery();
                        }
                    transaction.Commit();
                    }
                    
                    catch (Exception ex)
                    {
                    try { transaction.Rollback(); } catch { /* если уже неактивна — не падаем */ }
                    MessageBox.Show(
                                ex.Message,
                                "Database error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );

                        return "Updating quantity failed";
                    }


                
            }
            return null;
        }
        public static string Write_toDB(Auto auto)
        {

            connectionString = GetConnectionString();
            List<Auto> Cars = new List<Auto>();
            Cars = SelectAutoWindow.Get_cars();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                foreach (Auto c in Cars)
                {
                    if (auto.Name == c.Name && auto.Price == c.Price)
                    {
                        try
                        {
                            connection.Open();

                            string query = @"
                                UPDATE Auto
                                SET Quantity = Quantity + @Quantity
                                WHERE Id = @Id";

                            using (MySqlCommand command = new MySqlCommand(query, connection))
                            {
                               

                                command.Parameters.AddWithValue("@Id", c.Id);
                                command.Parameters.AddWithValue("@Quantity", auto.Quantity);

                                int rowsAffected = command.ExecuteNonQuery();
                                Thread.Sleep(1000);

                                if (rowsAffected > 0)
                                {
                                    return "Quantity of " + auto.Name + " successfully updated";
                                }
                                else
                                {
                                    return "Car with this ID was not found";
                                }
                            }
                        }
                        catch (MySqlException ex)
                        {
                            MessageBox.Show(
                                ex.Message,
                                "Database error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );

                            return "Updating quantity failed";
                        }
                        
                    }
                }

                try
                {
                    connection.Open();

                    string query = "INSERT INTO Auto (Name, Price, Category, Quantity, image)" +
                        " VALUES (@Name, @Price, @Category, @Quantity, @Image)";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        string name = auto.Name;
                        string category = auto.Category; // сделал строкой
                        int price = auto.Price;
                        int quantity = auto.Quantity;
                       
                        //string token = verification.GenerateToken();
                        //string token = Verification.GenerateToken();
                        //string token_hash = Verification.ComputeSha256Hash(token);
                        
                        //DateTime expireTime = DateTime.Now.AddMinutes(1); // текущее время + 3 минуты
                        //string token = Guid.NewGuid().ToString();
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Price", price);
                        command.Parameters.AddWithValue("@Category", category);
                        command.Parameters.AddWithValue("@Quantity", quantity);

                        // Картинка (BLOB). Если картинки нет — пишем NULL
                        command.Parameters.Add("@Image", MySqlDbType.MediumBlob).Value =
                            (object?)auto.ImageBytes ?? DBNull.Value;

                        int rowsAffected = command.ExecuteNonQuery();
                        Thread.Sleep(1000); // пауза 2000 мс (2 секунды)
                        //Verification.SendConfirmationEmail(email, token);
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
                    //Console.WriteLine("Ошибка: " + ex.Message);
                    MessageBox.Show(
                    ex.Message,
                    "Database error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                    // Код ошибки 1062 = Duplicate entry (уникальность нарушена)

                    if (ex.Number == 1062)
                    {
                        return "This email is already registered.";
                    }
                    else return "Sending is failure";
                    
                    
                }
                
            }
        }


    }
}