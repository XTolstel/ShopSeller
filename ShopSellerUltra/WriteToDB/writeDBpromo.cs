using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Write
{
    public class WriteDBPromo
    {
        static string connectionString;

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
