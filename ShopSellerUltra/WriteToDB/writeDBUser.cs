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
using AutoSellerUltra.Login;
using Microsoft.Extensions.Configuration;
//using System.Data.SqlClient;
//using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using static AutoSellerUltra.Login.UserSession;

namespace Write
{
    public class WriteDBUser
    {

        static string connectionString;

        public static void WriteAutoUserIfNotExists(UserDto User)
        {
            
                connectionString = WriteDB.GetConnectionString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // INSERT IGNORE: если нарушается уникальность (id уже есть) — команда не падает,
                    // просто вставка будет проигнорирована (rowsAffected = 0)
                    string query = @"
                    INSERT IGNORE INTO AutoUsers
                    (id, login, email, datebirth, balance, spendbalance)
                    VALUES
                    (@id, @login, @email, @datebirth, @balance, @spent);
                    ";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", User.Id);
                        command.Parameters.AddWithValue("@login", User.Login);
                        command.Parameters.AddWithValue("@email", User.Email);
                        command.Parameters.AddWithValue("@datebirth", User.DateOfBirth);
                        command.Parameters.AddWithValue("@balance", 0);
                        command.Parameters.AddWithValue("@spent", 0);

                        command.ExecuteNonQuery();
                        // если id уже был — rowsAffected будет 0, и это ОК (мы молчим)
                    }
                }
                catch
                {
                    // По твоему условию: не уведомлять. Можно логировать в консоль при желании.
                    // Console.WriteLine(ex);
                }
            }
        }

        public static (int balance, int spendBalance) LoadBalance(int id)
        {

            connectionString = WriteDB.GetConnectionString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                SELECT balance, spendbalance
                FROM AutoUsers
                WHERE id = @id
                LIMIT 1;
            ";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int balance = reader.GetInt32("balance");
                                int spent = reader.GetInt32("spendbalance");

                                return (balance, spent);
                            }
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

                    
                }
            }
            return (0, 0);
        }

            public static void UpdateBalance(UserDto User)
        {

            connectionString = WriteDB.GetConnectionString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // INSERT IGNORE: если нарушается уникальность (id уже есть) — команда не падает,
                    // просто вставка будет проигнорирована (rowsAffected = 0)
                    string query = @"
                    UPDATE AutoUsers
                    SET balance=@balance,
                        spendbalance = @spendbalance
                    WHERE id = @id
                    ";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", User.Id);
                        command.Parameters.AddWithValue("@balance", User.balance);
                        command.Parameters.AddWithValue("@spendbalance", User.spendbalance);

                        command.ExecuteNonQuery();
                        // если id уже был — rowsAffected будет 0, и это ОК (мы молчим)
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
