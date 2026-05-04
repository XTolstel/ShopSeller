using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Write
{
    public class WriteDBPromo
    {
        static string connectionString;

        public static void StartPromo()
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

                            string selectIdsSql = @"
                                SELECT id
                                FROM Promocodes
                                WHERE expiration_date IS NOT NULL
                                AND expiration_date < @now";

                            var expiredIds = new System.Collections.Generic.List<int>();
                            using (var selectCmd = new MySqlCommand(selectIdsSql, conn))
                            {
                                selectCmd.Parameters.AddWithValue("@now", DateTime.Now);

                                using (var reader = await selectCmd.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        //expiredIds.Add(reader.GetInt32("id"));
                                        expiredIds.Add(Convert.ToInt32(reader["id"]));
                                    }
                                }
                            }

                           

                            if (expiredIds.Count > 0)
                            { 
                                DeleteUsedPromo(expiredIds.ToArray());
                                string deleteSql = @"
                                    DELETE FROM Promocodes
                                    WHERE expiration_date IS NOT NULL
                                    AND expiration_date < @now";

                                using (var deleteCmd = new MySqlCommand(deleteSql, conn))
                                {
                                    deleteCmd.Parameters.AddWithValue("@now", DateTime.Now);

                                    int rowsAffected = await deleteCmd.ExecuteNonQueryAsync();
                                    Console.WriteLine($"Deleted {rowsAffected} expired promocodes at {DateTime.Now}");
                                }
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
        

        public static async Task<(bool IsValid, string State, int Discount, string Message)> CheckPromocodeAsync(string promoCode)
        {
            string sql = @"
                SELECT 
                    id,
                    code,
                    discount,
                    expiration_date
                FROM Promocodes
                WHERE code = @code
                LIMIT 1;
                ";
            connectionString = WriteDB.GetConnectionString();

            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@code", promoCode);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            return (
                                false,
                                "Invalid",
                                0,
                                "The promocode is invalid: this promocode does not exist."
                            );
                        }

                        int discount = reader.GetInt32("discount");
                        DateTime expirationDate = reader.GetDateTime("expiration_date");

                        if (expirationDate < DateTime.Now)
                        {
                            return (
                                false,
                                "Invalid",
                                discount,
                                "The promocode is invalid: it has expired."
                            );
                        }

                        return (
                            true,
                            "Valid",
                            discount,
                            "The promocode is valid."
                        );
                    }
                }
            }
        }

        public static async Task<int> DeleteUsedPromo(int[] promoIds)
        {
            connectionString = WriteDB.GetConnectionString();
            int deletedRows = 0;

            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();

                foreach (int promoId in promoIds)
                {
                    string sql = @"
                        DELETE FROM Used_Promocodes
                        WHERE promocode = (SELECT code FROM Promocodes WHERE id = @promoId LIMIT 1)";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@promoId", promoId);
                        deletedRows += await cmd.ExecuteNonQueryAsync();
                    }
                }
            }

            return deletedRows;
        }

        public static async Task SaveUsedPromocodeAsync(int userId, string promoCode)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(promoCode))
            {
                return;
            }

            connectionString = WriteDB.GetConnectionString();

            using (var conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string sql = @"
                    INSERT INTO Used_Promocodes (user_id, promocode)
                    VALUES (@userId, @code);";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@code", promoCode);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
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
