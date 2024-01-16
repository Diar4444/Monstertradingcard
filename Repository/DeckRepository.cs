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

        public DeckRepository(string username) 
        {
            this.username = username;
        }


        public string GetCardsFromDeckJson(string token)
        {
            List<Card> userDeck = new List<Card>();

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
            string jsonResult = JsonSerializer.Serialize(userDeck, new JsonSerializerOptions { WriteIndented = true });

            return jsonResult;
        }


    }
}
