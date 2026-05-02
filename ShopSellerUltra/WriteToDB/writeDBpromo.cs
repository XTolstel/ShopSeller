using System;
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

        private static void DeleteUsedPromo(int[] expiredIds)
        {
            if (expiredIds == null || expiredIds.Length == 0)
                return;

            Console.WriteLine($"Expired promocode ids: {string.Join(", ", expiredIds)}");
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
