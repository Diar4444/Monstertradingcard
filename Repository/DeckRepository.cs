using MonsterTradingCardGame.Objects;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.Repository
{
    public class DeckRepository
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=Halamadrid1;Database=postgres";

        private string username;

        private Card Card;


        public DeckRepository() { }


        public string GetCardsFromDeck(string token,bool format)
        {
            List<Card> userDeck = new List<Card>();
            string jsonResult = "";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(
                    "SELECT cards.id, cards.name, cards.damage " +
                    "FROM users " +
                    "JOIN user_packages ON users.username = user_packages.username " +
                    "JOIN cards ON user_packages.package_id = cards.package_id " +
                    "JOIN deck ON users.username = deck.username AND cards.id = deck.card_id " +
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
                            userDeck.Add(card);
                        }
                    }
                }
            }


            if (format)
            {
                StringBuilder plainTextBuilder = new StringBuilder();
                foreach (var card in userDeck)
                {
                    plainTextBuilder.AppendLine($"Id: {card.Id}, Name: {card.Name}, Damage: {card.Damage}");
                }

                jsonResult = plainTextBuilder.ToString();
            }
            else jsonResult = JsonSerializer.Serialize(userDeck, new JsonSerializerOptions { WriteIndented = true });

            return jsonResult;
        }

        public bool DoesCardBelongToUser(string token, string cardId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(
                    "SELECT COUNT(*) " +
                    "FROM users " +
                    "JOIN user_packages ON users.username = user_packages.username " +
                    "JOIN cards ON user_packages.package_id = cards.package_id " +
                    "WHERE users.token = @token AND cards.id = @cardId;", connection))
                {
                    command.Parameters.AddWithValue("@token", token);
                    command.Parameters.AddWithValue("@cardId", cardId);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    return count > 0;
                }
            }
        }

        public bool AddCardToUserDeck(string username, string cardId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(
                    "INSERT INTO deck (username, card_id) VALUES (@username, @cardId);", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@cardId", cardId);

                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
        }

        public bool DeleteUserDeck(string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand(
                    "DELETE FROM deck WHERE username = @username;", connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
        }
    }
}
