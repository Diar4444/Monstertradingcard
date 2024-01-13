using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using MonsterTradingCardGame.Objects;
using System.Drawing;


namespace MonsterTradingCardGame.Repository
{
    public class UserRepository
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=Halamadrid1;Database=postgres";
        User User { get; set; }

        public UserRepository() { }
        public UserRepository(User user)
        {
            User = user;
        }

        public void AddUser()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO users (token, username, password, coins) VALUES (@token, @username, @password, @coins)", connection))
                {
                    string token = User.Username + "-mtcgToken";

                    command.Parameters.AddWithValue("@token", token);
                    command.Parameters.AddWithValue("@username", User.Username);
                    command.Parameters.AddWithValue("@password", HashPassword(User.Password)); 
                    command.Parameters.AddWithValue("@coins", 20);

                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
        public bool DoesUserExist()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE username = @username;", connection))
                {
                    command.Parameters.AddWithValue("@username", User.Username);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    connection.Close();
                    return count > 0;
                }
            }
        }

        public bool UserLogin()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT username, password FROM users WHERE username = @username;", connection))
                {
                    command.Parameters.AddWithValue("@username", User.Username);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedUsername = reader.GetString(0);
                            string storedPasswordHash = reader.GetString(1);

                            // Check if the provided password matches the stored hashed password
                            if (HashPassword(User.Password) == storedPasswordHash && User.Username == storedUsername)
                            {
                                // Passwords match
                                connection.Close();
                                return true;
                            }
                        }
                    }

                    // No matching user or incorrect password
                    connection.Close();
                    return false;
                }
            }
        }

        public int GetCoins(string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT coins FROM users WHERE username = @username;", connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int storedCoins = reader.GetInt32(0); 
                            connection.Close();
                            return storedCoins;
                        }
                    }
                    connection.Close();
                    return -1;
                }
            }
        }

        public bool UpdateCoins(int coins,string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("UPDATE users SET coins = @newCoins WHERE username = @username;", connection))
                {

                    command.Parameters.AddWithValue("@newCoins", coins);
                    command.Parameters.AddWithValue("@username", username);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // Update successful
                        connection.Close();
                        return true;
                    }
                }

                // No matching user
                connection.Close();
                return false;
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

    }
}
