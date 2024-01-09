using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace MonsterTradingCardGame
{
    public class UserRepository
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=Halamadrid1;Database=postgres";
        User User { get; set; }

        public UserRepository(User user)
        {
            User = user;
        }

        public void AddUser()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO users (username, password) VALUES (@username, @password);", connection))
                {
                    command.Parameters.AddWithValue("@username", User.Username);
                    command.Parameters.AddWithValue("@password", HashPassword(User.Password));

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

                    // Check if the count is greater than 0
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
