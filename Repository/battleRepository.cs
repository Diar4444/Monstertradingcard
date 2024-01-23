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
    public class battleRepository
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

        public battleRepository()  { }

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

                if (connectedClientCount == 0) username1 = requestHandler.GetUsernameByToken(token);;
                
                if (connectedClientCount == 1) username2 = requestHandler.GetUsernameByToken(token);

                lock (clientLock)
                {
                    connectedClientCount++;

                    if (connectedClientCount == 2)
                    {
                        semaphore.Release(2); // Release both permits
                    }
                }

                await semaphore.WaitAsync(); // Wait for the signal to proceed

                lock (clientLock)
                {
                    connectedClientCount = 0; // Reset the connectedClientCount
                }

                if (username1 == username2) return resp = Encoding.UTF8.GetBytes(responseMsg.GetResponseMessage(402));

                if (username1 == requestHandler.GetUsernameByToken(token)) return resp = Encoding.UTF8.GetBytes(responseMsg.GetResponseMessage(200));



                Console.WriteLine("Proceeding with the battle.");

                Battle();

                resp = Encoding.UTF8.GetBytes(responseMsg.GetResponseMessage(200));
            }
            else
            {
                resp = Encoding.UTF8.GetBytes(responseMsg.GetResponseMessage(401));
            }

            return resp;
        }

        private static List<(string Name, double Damage)> GetCardsInfo(string username)
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


        private static void Battle()
        {
            List<(string,double)> user1Cards = GetCardsInfo(username1);
            List<(string,double)> user2Cards = GetCardsInfo(username2);

            Console.WriteLine(username1 + "+" + username2);

            for(int i = 0; i < 100; i++)
            {
                Random random = new Random();
                int randomNumber = random.Next(0,4);

                Console.WriteLine("Battle: "+i);
                Console.WriteLine("usercards1: "+ user1Cards[randomNumber].Item2);
                Console.WriteLine("usercards2: " + user2Cards[randomNumber].Item2);
            }


        }
    }
}
