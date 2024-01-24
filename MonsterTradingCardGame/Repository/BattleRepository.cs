using MonsterTradingCardGame.Objects;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.Repository
{
    public class BattleRepository
    {
        private static readonly object clientLock = new object();
        private static int connectedClientCount = 0;
        private static SemaphoreSlim semaphore = new SemaphoreSlim(0, 2);

        private static string host = "localhost";
        private static string username = "postgres";
        private static string password = "Halamadrid1";
        private static string database = "postgres";
        private static string getConnectionString()
        {
            return "Host=" + host + ";Username=" + username + ";Password=" + password + ";Database=" + database;
        }

        private static string username1;
        private static string username2;

        private static List<(string, double)> user1Cards;
        private static List<(string, double)> user2Cards;

        public BattleRepository() { }

        public static async Task<byte[]> ProcessBattlesRequestAsync(string request)
        {
            byte[] resp;
            ResponseMsg responseMsg = new ResponseMsg("battle");
            RequestHandler requestHandler = new RequestHandler();
            UserRepository userRepository = new UserRepository();

            string token = requestHandler.ExtractAuthorizationToken(request);


            if (userRepository.DoesTokenExist(token))
            {
                Console.WriteLine("Client connected!");

                if (connectedClientCount == 0) username1 = requestHandler.GetUsernameByToken(token); ;

                if (connectedClientCount == 1) username2 = requestHandler.GetUsernameByToken(token);

                lock (clientLock)
                {
                    connectedClientCount++;

                    if (connectedClientCount == 2)
                    {
                        semaphore.Release(2);
                    }
                }

                await semaphore.WaitAsync(); 

                lock (clientLock)
                {
                    connectedClientCount = 0; 
                }

                //Falls zweimal der gleich token eingegeben wird
                if (username1 == username2) return resp = Encoding.UTF8.GetBytes(responseMsg.GetResponseMessage(402));

                //Es laufen ja 2 Tasks und der erste wird einfach rausgeschmissen und der zweite fahrt dann fort
                if (username1 == requestHandler.GetUsernameByToken(token)) return resp = Encoding.UTF8.GetBytes(responseMsg.GetResponseMessage(200));

                Console.WriteLine("Proceeding with the battle.");

                string output = Battle();

                resp = Encoding.UTF8.GetBytes(responseMsg.GetResponseMessage(200) + output + "\r\n");
            }
            else
            {
                resp = Encoding.UTF8.GetBytes(responseMsg.GetResponseMessage(401));
            }

            return resp;
        }

        public static List<(string Name, double Damage)> GetCardsInfo(string username)
        {
            List<(string Name, double Damage)> deck = new List<(string, double)>();

            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand($"SELECT cards.name, cards.damage FROM deck INNER JOIN cards ON deck.card_id = cards.id WHERE deck.username = '{username}'", connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string cardName = reader["name"].ToString();
                            double damage = Convert.ToDouble(reader["damage"]);

                            deck.Add((cardName, damage));
                        }
                    }
                }

                connection.Close();
            }

            return deck;
        }


        public static string Battle()
        {
            user1Cards = GetCardsInfo(username1);
            user2Cards = GetCardsInfo(username2);
            string response = "";

            Console.WriteLine(username1 + "+" + username2);
            Random random = new Random();

            for (int i = 0; i < 100; i++)
            {
                int rmbr1 = random.Next(0, user1Cards.Count);
                int rmbr2 = random.Next(0, user2Cards.Count);

                double damageUser1 = user1Cards[rmbr1].Item2;
                double damageUser2 = user2Cards[rmbr2].Item2;
                string nameUser1 = user1Cards[rmbr1].Item1;
                string nameUser2 = user2Cards[rmbr2].Item1;

                Console.WriteLine();
                for (int a = 0; a < 50; a++)Console.Write('-');
                Console.WriteLine();
                Console.WriteLine("Round: " + i);
                response += "-----------------------\n";
                response += "Round: " + i + "\n";


                if (IsMonsterCard(nameUser1) && IsMonsterCard(nameUser2))
                {
                    // Monster Fights
                    response += HandleMonsterFight(nameUser1, damageUser1, nameUser2, damageUser2, rmbr1, rmbr2);
                }
                else if (IsSpellCard(nameUser1) && IsSpellCard(nameUser2))
                {
                    // Spell Fights
                    response += HandleSpellFight(nameUser1, damageUser1, nameUser2, damageUser2, rmbr1, rmbr2);
                }
                else
                {
                    // Mixed Fights
                    response += HandleMixedFight(nameUser1, damageUser1, nameUser2, damageUser2, rmbr1, rmbr2);
                }

                if (user2Cards.Count == 0)
                {
                    Console.WriteLine("User 1 wins!");
                    i = 100;
                    response += "User: " + username1 + " wins!";
                    UpdateUserWinner(username1);
                    UpdateUserLosses(username2);

                }
                else if (user1Cards.Count == 0)
                {
                    Console.WriteLine("User 2 wins!");
                    i = 100;
                    response += "User: " + username2 + " wins!";
                    UpdateUserWinner(username2);
                    UpdateUserLosses(username1);
                }

            }
            return response;
        }

        private static string HandleMonsterFight(string nameUser1, double damageUser1, string nameUser2, double damageUser2, int rmbr1, int rmbr2)
        {
            string response = $"{nameUser1} vs {nameUser2}\n";
            Console.WriteLine($"{nameUser1} vs {nameUser2}");

            if (nameUser1 == "Goblin" && nameUser2 == "Dragon")
            {
                response += "Goblins are too afraid of Dragons to attack. Dragon wins!\n";
                user1Cards.Add(user2Cards[rmbr2]);
                user2Cards.RemoveAt(rmbr2);
            }
            else if (nameUser1 == "Wizard" && nameUser2 == "Orc")
            {
                response += "Wizard can control Orcs, so they are not able to damage them. Wizard wins!\n";
                user2Cards.Add(user1Cards[rmbr1]);
                user1Cards.RemoveAt(rmbr1);
            }
            else if (nameUser1 == "Knight" && nameUser2 == "WaterSpell")
            {
                response += "The armor of Knights is so heavy that WaterSpells make them drown instantly. WaterSpell wins!\n";
                user2Cards.Add(user1Cards[rmbr1]);
                user1Cards.RemoveAt(rmbr1);
            }
            else if (nameUser1 == "Dragon" && nameUser2 == "FireElf")
            {
                response += "The FireElves know Dragons since they were little and can evade their attacks. FireElf wins!\n";
                user1Cards.Add(user2Cards[rmbr2]);
                user2Cards.RemoveAt(rmbr2);
            }
            else if (nameUser1 == "Kraken")
            {
                response += "The Kraken is immune against spells. Kraken wins!\n";
                user1Cards.Add(user2Cards[rmbr2]);
                user2Cards.RemoveAt(rmbr2);
            }
            else if (nameUser2 == "Kraken")
            {
                response += "The Kraken is immune against spells. Kraken wins!\n";
                user2Cards.Add(user1Cards[rmbr1]);
                user1Cards.RemoveAt(rmbr1);
            }
            else
            {
                if (damageUser1 > damageUser2)
                {
                    response += $"{nameUser1} defeats {nameUser2}\n";
                    Console.WriteLine($"{nameUser1} defeats {nameUser2}");
                    user1Cards.Add(user2Cards[rmbr2]);
                    user2Cards.RemoveAt(rmbr2);
                }
                else if (damageUser1 < damageUser2)
                {
                    response += $"{nameUser2} defeats {nameUser1}\n";
                    Console.WriteLine($"{nameUser2} defeats {nameUser1}");
                    user2Cards.Add(user1Cards[rmbr1]);
                    user1Cards.RemoveAt(rmbr1);
                }
                else
                {
                    response += $"Draw! No action taken.\n";
                    Console.WriteLine($"Draw! No action taken.");
                }
            }
            return response;
        }

        private static string HandleSpellFight(string nameUser1, double damageUser1, string nameUser2, double damageUser2, int rmbr1, int rmbr2)
        {
            Console.WriteLine($"{nameUser1} vs {nameUser2}");
            string response = $"{nameUser1} vs {nameUser2}\n";

            double effectiveness = CalculateEffectiveness(nameUser1, nameUser2);

            double effectiveDamageUser1 = damageUser1 * effectiveness;
            double effectiveDamageUser2 = damageUser2 * effectiveness;

            if (effectiveDamageUser1 > effectiveDamageUser2)
            {
                Console.WriteLine($"{nameUser1} wins with {effectiveDamageUser1} damage!");
                user1Cards.Add(user2Cards[rmbr2]);
                user2Cards.RemoveAt(rmbr2);
                response += $"{nameUser1} wins with {effectiveDamageUser1} damage!\n";
            }
            else if (effectiveDamageUser1 < effectiveDamageUser2)
            {
                Console.WriteLine($"{nameUser2} wins with {effectiveDamageUser2} damage!");
                user2Cards.Add(user1Cards[rmbr1]);
                user1Cards.RemoveAt(rmbr1);
                response += $"{nameUser2} wins with {effectiveDamageUser2} damage!\n";
            }
            else
            {
                response += $"Draw! No action taken.\n";
                Console.WriteLine($"Draw! No action taken.");
            }
            return response;
        }

        private static string HandleMixedFight(string nameUser1, double damageUser1, string nameUser2, double damageUser2, int rmbr1, int rmbr2)
        {
            string response = $"{nameUser1} vs {nameUser2}\n";
            Console.WriteLine($"{nameUser1} vs {nameUser2}");

            if (nameUser1 == "Goblin" && nameUser2 == "Dragon")
            {
                response += "Goblins are too afraid of Dragons to attack. Dragon wins!\n";
                user1Cards.Add(user2Cards[rmbr2]);
                user2Cards.RemoveAt(rmbr2);
            }
            else if (nameUser1 == "Wizard" && nameUser2 == "Orc")
            {
                response += "Wizard can control Orcs, so they are not able to damage them. Wizard wins!\n";
                user2Cards.Add(user1Cards[rmbr1]);
                user1Cards.RemoveAt(rmbr1);
            }
            else if (nameUser1 == "Knight" && nameUser2 == "WaterSpell")
            {
                response += "The armor of Knights is so heavy that WaterSpells make them drown instantly. WaterSpell wins!\n";
                user2Cards.Add(user1Cards[rmbr1]);
                user1Cards.RemoveAt(rmbr1);
            }
            else if (nameUser1 == "Dragon" && nameUser2 == "FireElf")
            {
                response += "The FireElves know Dragons since they were little and can evade their attacks. FireElf wins!\n";
                user1Cards.Add(user2Cards[rmbr2]); 
                user2Cards.RemoveAt(rmbr2);
            }
            else if (nameUser1 == "Kraken")
            {
                response += "The Kraken is immune against spells. Kraken wins!\n";
                user1Cards.Add(user2Cards[rmbr2]); 
                user2Cards.RemoveAt(rmbr2);
            }
            else if (nameUser2 == "Kraken")
            {
                response += "The Kraken is immune against spells. Kraken wins!\n";
                user2Cards.Add(user1Cards[rmbr1]); 
                user1Cards.RemoveAt(rmbr1);
            }
            else
            {
                if (IsSpellCard(nameUser1) || IsSpellCard(nameUser2))
                {
                    double effectiveness = CalculateEffectiveness(nameUser1, nameUser2);

                    damageUser1 *= effectiveness;
                    damageUser2 *= effectiveness;
                }

                if (damageUser1 > damageUser2)
                {
                    user1Cards.Add(user2Cards[rmbr2]); 
                    user2Cards.RemoveAt(rmbr2);
                    response += $"{nameUser1} wins with {damageUser1} damage!\n";
                }
                else if (damageUser1 < damageUser2)
                {
                    user2Cards.Add(user1Cards[rmbr1]); 
                    user1Cards.RemoveAt(rmbr1);
                    response += $"{nameUser2} wins with {damageUser2} damage!\n";
                }
                else
                {
                    Console.WriteLine($"Draw! No action taken.");
                    response += $"Draw! No action taken.\n";
                }
            }
            return response;
        }

        public static double CalculateEffectiveness(string nameUser1, string nameUser2)
        {
            Dictionary<string, string> effectivenessRules = new Dictionary<string, string>
            {
                { "Water", "Fire" },
                { "Fire", "Normal" },
                { "Normal", "Water" }
            };

            if (effectivenessRules.TryGetValue(nameUser1, out string effectivenessAgainst))
            {
                if (effectivenessAgainst == nameUser2)
                {
                    return 2.0;
                }
                else if (effectivenessRules.TryGetValue(nameUser2, out string effectivenessAgainstOpponent) && effectivenessAgainstOpponent == nameUser1)
                {
                    return 0.5;
                }
            }

            return 1.0;
        }

        public static bool IsMonsterCard(string cardName)
        {
            return cardName.EndsWith("Goblin") || cardName.EndsWith("Troll") || cardName.EndsWith("Wizard") || cardName.EndsWith("Knight") || cardName.EndsWith("Dragon") || cardName.EndsWith("FireElf") || cardName.EndsWith("Kraken");
        }

        public static bool IsSpellCard(string cardName)
        {
            return cardName.EndsWith("Spell");
        }

        public static void UpdateUserWinner(string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("UPDATE users SET elo = elo + @eloChange, wins = wins + @winsChange WHERE username = @username;", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@eloChange", 3);
                    command.Parameters.AddWithValue("@winsChange", 1);

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        private static void UpdateUserLosses(string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(getConnectionString()))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("UPDATE users SET elo = elo - @eloChange, losses = losses + @lossesChange WHERE username = @username;", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@eloChange", 5);
                    command.Parameters.AddWithValue("@lossesChange", 1);

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

    }
}
