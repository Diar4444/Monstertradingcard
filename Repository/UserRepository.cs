using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using MonsterTradingCardGame.Objects;
using System.Drawing;
using System.Xml;
using System.Text.Json;



namespace MonsterTradingCardGame.Repository
{
    public class UserRepository
    {
        private string host = "localhost";
        private string username = "postgres";
        private string password = "Halamadrid1";
        private string database = "postgres";
        private string getConnectionString()
        {
            return "Host=" + host + ";Username=" + username + ";Password=" + password + ";Database=" + database;
        }
        private User User { get; set; }

        public UserRepository() { }
        public UserRepository(User user)
        {
            User = user;
        }

        public void AddUser()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO users (token, username, password, coins, elo, wins, losses) VALUES (@token, @username, @password, @coins, 100, 0, 0)", connection))
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
        public bool DoesUserExist(string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE username = @username;", connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    connection.Close();
                    return count > 0;
                }
            }
        }

        public bool UserLogin()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
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
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
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
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("UPDATE users SET coins = @newCoins WHERE username = @username;", connection))
                {
                    command.Parameters.AddWithValue("@newCoins", coins);
                    command.Parameters.AddWithValue("@username", username);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        connection.Close();
                        return true;
                    }
                }

                // No matching user
                connection.Close();
                return false;
            }
        }

        public string GetUserCardsJSON(string token)
        {
            List<Card> userCards = new List<Card>();

            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(
                    "SELECT cards.id, cards.name, cards.damage " +
                    "FROM users " +
                    "JOIN user_packages ON users.username = user_packages.username " +
                    "JOIN cards ON user_packages.package_id = cards.package_id " +
                    "WHERE users.token = @token;", connection))
                    { 
                    command.Parameters.AddWithValue("@token", token);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Card card = new Card
                            {
                                Id = reader.GetString(0),
                                Name = reader.GetString(1),
                                Damage = reader.GetDouble(2)
                            };

                            userCards.Add(card);
                        }
                    }
                }

                string jsonResult = JsonSerializer.Serialize(userCards, new JsonSerializerOptions{ WriteIndented = true });

                return jsonResult;
            }
        }

        public bool DoesTokenExist(string token)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE token = @token;", connection))
                {
                    command.Parameters.AddWithValue("@token", token);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    connection.Close();
                    return count > 0;
                }
            }
        }
        public bool IsTokenValidForUsername(string username, string token)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM users WHERE username = @username AND token = @token;", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@token", token);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    return count > 0;
                }
            }
        }

        public string GetUserData(string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(
                    "SELECT image, bio, name FROM users WHERE username = @username;", connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            User user = new User
                            {
                                Image = reader.IsDBNull(0) ? null : reader.GetString(0),
                                Bio = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Name = reader.IsDBNull(2) ? null : reader.GetString(2)
                            };

                            var result = new
                            {
                                Bio = user.Bio,
                                Image = user.Image,
                                Name = user.Name
                            };

                            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                        }
                    }
                }
            }

            return "{}";
        }

        public bool UpdateUserData(string username, string jsonUserData)
        {
            try
            {
                User userData = JsonSerializer.Deserialize<User>(jsonUserData);

                using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(
                        "UPDATE users SET name = @name, bio = @bio, image = @image WHERE username = @username;", connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@name", userData.Name);
                        command.Parameters.AddWithValue("@bio", userData.Bio);
                        command.Parameters.AddWithValue("@image", userData.Image);

                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected > 0;
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                return false;
            }
        }

        public string GetUserStats(string token)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(
                        "SELECT name, elo, wins, losses FROM users WHERE token = @token;", connection))
                    {
                        command.Parameters.AddWithValue("@token", token);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var userStats = new
                                {
                                    Name = reader.IsDBNull(0) ? null : reader.GetString(0),
                                    Elo = reader.GetInt32(1),
                                    Wins = reader.GetInt32(2),
                                    Losses = reader.GetInt32(3)
                                };

                                return JsonSerializer.Serialize(userStats, new JsonSerializerOptions { WriteIndented = true });
                            }
                        }
                    }
                }

                return "{}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user stats: {ex.Message}");
                return "{}";
            }
        }

        public string GetAllUserStatsOrderedByElo()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(
                        "SELECT name, elo, wins, losses FROM users ORDER BY elo DESC;", connection))
                    {
                        List<object> userStatsList = new List<object>();

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var userStats = new
                                {
                                    Name = reader.IsDBNull(0) ? null : reader.GetString(0),
                                    Elo = reader.GetInt32(1),
                                    Wins = reader.GetInt32(2),
                                    Losses = reader.GetInt32(3)
                                };

                                userStatsList.Add(userStats);
                            }
                        }

                        return JsonSerializer.Serialize(userStatsList, new JsonSerializerOptions { WriteIndented = true });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all user stats: {ex.Message}");
                return "[]";
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
