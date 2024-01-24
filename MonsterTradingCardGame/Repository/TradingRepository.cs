using MonsterTradingCardGame.Objects;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.Repository
{
    public class TradingRepository
    {
        private string host = "localhost";
        private string username = "postgres";
        private string password = "Halamadrid1";
        private string database = "postgres";
        private string getConnectionString()
        {
            return "Host=" + host + ";Username=" + username + ";Password=" + password + ";Database=" + database;
        }

        public TradingRepository() { }


        public string GetTrades()
        {
            List<Trade> serializedTrades = new List<Trade>();

            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT id, cardtotrade, card_type, minimumdamage, username FROM tradings;", connection))
                {

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Trade serializedTrade = new Trade
                            {
                                Id = reader.GetString(0),
                                CardToTrade = reader.GetString(1),
                                Type = reader.GetString(2),
                                MinimumDamage = reader.GetDouble(3)
                            };

                            serializedTrades.Add(serializedTrade);
                        }
                    }
                }

                connection.Close();
            }

            string jsonResult = JsonSerializer.Serialize(serializedTrades, new JsonSerializerOptions { WriteIndented = true });

            Console.WriteLine(jsonResult);

            return jsonResult;
        }

        public bool DoesUserHaveCard(string username, string cardId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT cards.id FROM user_packages INNER JOIN cards ON user_packages.package_id = cards.package_id WHERE user_packages.username = @username AND cards.id = @cardId;", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@cardId", cardId);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        bool hasResult = reader.Read();

                        connection.Close();

                        return hasResult;
                    }
                }
            }
        }

        public bool DoesCardExistInTrading(string cardId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT id FROM tradings WHERE cardtotrade = @cardId;", connection))
                {
                    command.Parameters.AddWithValue("@cardId", cardId);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        bool exists = reader.Read();

                        connection.Close();

                        return exists;
                    }
                }
            }
        }

        public bool DoesIdBelongToUser(string id,string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT id FROM tradings WHERE id =  @id AND username =  @username;", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@username", username);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        bool exists = reader.Read();

                        connection.Close();

                        return exists;
                    }
                }
            }
        }

        public bool DoesIdExists(string Id)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT id FROM tradings WHERE id = @Id;", connection))
                {
                    command.Parameters.AddWithValue("@Id", Id);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        bool exists = reader.Read();

                        connection.Close();

                        return exists;
                    }
                }
            }
        }

        public void AddTrade(string cardToTrade, string Id, string cardType, double minimumDamage, string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO tradings (id, cardtotrade, card_type, minimumdamage, username) VALUES (@Id, @cardToTrade, @cardType, @minimumDamage, @username);", connection))
                {
                    command.Parameters.AddWithValue("@Id", Id);
                    command.Parameters.AddWithValue("@cardToTrade", cardToTrade);
                    command.Parameters.AddWithValue("@cardType", cardType);
                    command.Parameters.AddWithValue("@minimumDamage", minimumDamage);
                    command.Parameters.AddWithValue("@username", username);

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
        public void DeleteTradeById(string tradeId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("DELETE FROM tradings WHERE id = @tradeId;", connection))
                {
                    command.Parameters.AddWithValue("@tradeId", tradeId);

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        public bool CheckDamageSufficient(string cardId, string tradingId)
        {
            double cardDamage = GetCardDamage(cardId);
            double minimumDamageInOffer = GetMinimumDamageInOffer(tradingId);

            return cardDamage >= minimumDamageInOffer;
        }

        public string GetCardToTradeById(string tradingId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT cardtotrade FROM tradings WHERE id = @tradingId;", connection))
                {
                    command.Parameters.AddWithValue("@tradingId", tradingId);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(0);
                        }
                    }
                }

                connection.Close();
            }

            return null;
        }
        private double GetCardDamage(string cardId)
        {
            double cardDamage = 0.0;

            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT damage FROM cards WHERE id = @cardId;", connection))
                {
                    command.Parameters.AddWithValue("@cardId", cardId);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cardDamage = reader.GetDouble(0);
                        }
                    }
                }

                connection.Close();
            }

            return cardDamage;
        }

        private double GetMinimumDamageInOffer(string tradingId)
        {
            double minimumDamageInOffer = 0.0;

            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT minimumdamage FROM tradings WHERE id = @tradingId;", connection))
                {
                    command.Parameters.AddWithValue("@tradingId", tradingId);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            minimumDamageInOffer = reader.GetDouble(0);
                        }
                    }
                }

                connection.Close();
            }

            return minimumDamageInOffer;
        }

        public int GetPackageIdFromCardId(string cardId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT package_id FROM cards WHERE id = @cardId;", connection))
                {
                    command.Parameters.AddWithValue("@cardId", cardId);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(0);
                        }
                    }
                }

                connection.Close();
            }
            return 0;
        }

        public bool UpdatePackageIdForCard(string cardIdToUpdate, int newPackageId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("UPDATE cards SET package_id = @newPackageId WHERE id = @cardIdToUpdate;", connection))
                {
                    command.Parameters.AddWithValue("@cardIdToUpdate", cardIdToUpdate);
                    command.Parameters.AddWithValue("@newPackageId", newPackageId);

                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0; 
                }
            }
        }
    }
}
