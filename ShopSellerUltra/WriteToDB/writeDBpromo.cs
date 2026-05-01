using System;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Write
{
    public class WriteDBPromo
    {
        static string connectionString;

        public static void Start_Promo()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        connectionString = WriteDB.GetConnectionString();

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

        public static void AddPromocode(string code, int discount, DateTime expirationDate)
        {
            connectionString = WriteDB.GetConnectionString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                    INSERT INTO Promocodes
                    (code, discount, expiration_date)
                    VALUES
                    (@code, @discount, @expiration_date);
                    ";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@code", code);
                        command.Parameters.AddWithValue("@discount", discount);
                        command.Parameters.AddWithValue("@expiration_date", expirationDate.Date);
                        command.ExecuteNonQuery();
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
                }
            }
        }
    }
}
