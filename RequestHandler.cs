using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MonsterTradingCardGame.Objects;
using MonsterTradingCardGame.Repository;

namespace MonsterTradingCardGame
{
    public class RequestHandler
    {
        public string request { get; private set; }

        public string response { get; private set; }

        private const string adminToken = "admin-mtcgToken";

        private PackageRepository packageRepository = new PackageRepository();

        private UserRepository userRepository = new UserRepository();

        private DeckRepository deckRepository = new DeckRepository();

        public RequestHandler() { }

        public RequestHandler(string Request)
        {
            request = Request;

            Handler();
        }

        private void Handler()
        {
            try
            {
                string authenticationToken = ExtractAuthorizationToken(request);
                int bodyStartIndex = request.IndexOf("{");

                if (request.Contains("POST /packages") || request.Contains("PUT /deck")) bodyStartIndex = request.IndexOf("[");

                if (bodyStartIndex >= 0)
                {
                    // Find the end of the HTTP headers
                    int headerEndIndex = request.IndexOf("\r\n\r\n") + 4;

                    // Extract the Content-Length header value
                    string contentLengthHeader = request.Substring(request.IndexOf("Content-Length:") + 15);
                    int contentLength = int.Parse(contentLengthHeader.Substring(0, contentLengthHeader.IndexOf("\r\n")));

                    // Extract the JSON payload based on Content-Length
                    var jsonPayload = request.Substring(bodyStartIndex, contentLength);

                    if (request.Contains("POST /users") || request.Contains("POST /sessions"))
                    {
                        // Parse JSON payload
                        var userObject = JsonSerializer.Deserialize<User>(jsonPayload);

                        UserRepository userrep = new UserRepository(userObject);

                        // Check the endpoint and perform specific logic
                        if (request.Contains("POST /users"))
                        {
                            ResponseMsg responseMsg = new ResponseMsg("users");
                            if (!userrep.DoesUserExist(userObject.Username))
                            {
                                userrep.AddUser();

                                response = responseMsg.GetResponseMessage(201);
                            }
                            else response = responseMsg.GetResponseMessage(409);
                        }
                        else if (request.Contains("POST /sessions"))
                        {
                            ResponseMsg responseMsg = new ResponseMsg("sessions");
                            if (userrep.UserLogin())
                            {
                                response = responseMsg.GetResponseMessage(200) + "Token: " + userObject.Username + "-mtcgToken\r\n";
                            }
                            else
                            {
                                response = responseMsg.GetResponseMessage(401);
                            }
                        }
                    }
                    else if (request.Contains("POST /packages"))
                    {
                        ResponseMsg responseMsg = new ResponseMsg("packages");
                        if (authenticationToken.Length > 0)
                        {
                            if (authenticationToken == adminToken)
                            {
                                int lastPackageID = packageRepository.GetPackageId();

                                // Deserialize the JSON payload into a list of cards
                                List<Card> cards = JsonSerializer.Deserialize<List<Card>>(jsonPayload);

                                // Create a package
                                var package = new Package { PackageId = lastPackageID, Bought = false };

                                // Add the cards to the package
                                package.Cards.AddRange(cards);

                                packageRepository.AddPackage(package);

                                response = responseMsg.GetResponseMessage(201);
                            }
                            else response = responseMsg.GetResponseMessage(403);
                        }
                        else response = responseMsg.GetResponseMessage(401);
                    }
                    else if (request.Contains("PUT /deck"))
                    {
                        ResponseMsg responseMsg = new ResponseMsg("deckPUT");

                        if (userRepository.DoesTokenExist(authenticationToken))
                        {
                            List<string> cardIds = JsonSerializer.Deserialize<List<string>>(jsonPayload);
                            bool cardnotmine = false;
                            if (cardIds.Count == 4)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    if (deckRepository.DoesCardBelongToUser(authenticationToken, cardIds[i]) == false) cardnotmine = true;
                                }

                                if (!cardnotmine)
                                {
                                    deckRepository.DeleteUserDeck(GetUsernameByToken(authenticationToken));
                                    for (int i = 0; i < 4; i++)
                                    {
                                        deckRepository.AddCardToUserDeck(GetUsernameByToken(authenticationToken), cardIds[i]);
                                    }

                                    response = responseMsg.GetResponseMessage(200);
                                }
                                else response = responseMsg.GetResponseMessage(403);
                            }
                            else response = responseMsg.GetResponseMessage(400);
                        }
                        else response = responseMsg.GetResponseMessage(401);
                    }
                    else if (request.Contains("PUT /users"))
                    {
                        ResponseMsg responseMsg = new ResponseMsg("userPUT");

                        string username = "";
                        int start = request.IndexOf("/users/");

                        if (start != -1)
                        {
                            int usernameStart = start + ("/users/").Length;
                            int spaceIndex = request.IndexOf(' ', usernameStart);

                            if (spaceIndex != -1)
                            {
                                username = request.Substring(usernameStart, spaceIndex - usernameStart);
                            }
                        }

                        if (userRepository.DoesUserExist(username))
                        {
                            if (userRepository.IsTokenValidForUsername(username, authenticationToken) || authenticationToken == adminToken)
                            {
                                userRepository.UpdateUserData(username, jsonPayload);

                                response = responseMsg.GetResponseMessage(200);
                            }
                            else response = responseMsg.GetResponseMessage(401);
                        }
                        else response = responseMsg.GetResponseMessage(404);
                    }
                }
                else if (request.Contains("POST /transactions/packages"))
                {
                    ResponseMsg responseMsg = new ResponseMsg("transactions/packages");
                    List<int> packagelist = new List<int>();
                    int coins;
                    string username = GetUsernameByToken(authenticationToken);

                    //Ersatz weil das mit token im curl script komisch ist such ich nach username direkt

                    coins = userRepository.GetCoins(username);

                    packagelist = packageRepository.IsPackageAvailable();

                    if (packagelist.Count > 0)
                    {
                        if (coins >= 5)
                        {
                            userRepository.UpdateCoins(coins - 5, username);

                            packageRepository.BuyPackage(packagelist[0], username);

                            response = responseMsg.GetResponseMessage(200);
                        }
                        else
                        {
                            response = responseMsg.GetResponseMessage(403);
                        }
                    }
                    else
                    {
                        response = responseMsg.GetResponseMessage(404);
                    }
                }
                else if (request.Contains("GET /cards"))
                {
                    ResponseMsg responseMsg = new ResponseMsg("cards");
                    if (userRepository.DoesTokenExist(authenticationToken))
                    {
                        string userCards = userRepository.GetUserCardsJSON(authenticationToken);

                        if (userCards.Length > 2)
                        {
                            response = responseMsg.GetResponseMessage(200) + userCards + "\r\n";
                        }
                        else
                        {
                            response = responseMsg.GetResponseMessage(204);
                        }
                    }
                    else response = responseMsg.GetResponseMessage(401);
                }
                else if (request.Contains("GET /deck"))
                {
                    ResponseMsg responseMsg = new ResponseMsg("deckGET");
                    if (userRepository.DoesTokenExist(authenticationToken))
                    {
                        string showdeck = "";

                        if (request.Contains("format=plain")) showdeck = deckRepository.GetCardsFromDeck(authenticationToken, true);
                        else showdeck = deckRepository.GetCardsFromDeck(authenticationToken, false);

                        if (showdeck.Length > 2)
                        {
                            response = responseMsg.GetResponseMessage(200) + showdeck + "\r\n";
                        }
                        else
                        {
                            response = responseMsg.GetResponseMessage(204);
                        }
                    }
                    else response = responseMsg.GetResponseMessage(401);
                }
                else if (request.Contains("GET /users"))
                {
                    ResponseMsg responseMsg = new ResponseMsg("userGET");

                    string username = "";
                    int start = request.IndexOf("/users/");

                    if (start != -1)
                    {
                        int usernameStart = start + ("/users/").Length;
                        int spaceIndex = request.IndexOf(' ', usernameStart);

                        if (spaceIndex != -1)
                        {
                            username = request.Substring(usernameStart, spaceIndex - usernameStart);
                        }
                    }

                    if (userRepository.DoesUserExist(username))
                    {
                        if (userRepository.IsTokenValidForUsername(username, authenticationToken) || authenticationToken == adminToken)
                        {
                            string userData = userRepository.GetUserData(username);

                            response = responseMsg.GetResponseMessage(200) + userData + "\r\n";
                        }
                        else response = responseMsg.GetResponseMessage(401);
                    }
                    else response = responseMsg.GetResponseMessage(404);
                }
                else if (request.Contains("GET /stats"))
                {
                    ResponseMsg responseMsg = new ResponseMsg("stats");

                    if (userRepository.DoesTokenExist(authenticationToken))
                    {
                        response = responseMsg.GetResponseMessage(200) + userRepository.GetUserStats(authenticationToken) + "\r\n";

                    }
                    else response = responseMsg.GetResponseMessage(401);
                }
                else if (request.Contains("GET /scoreboard"))
                {
                    ResponseMsg responseMsg = new ResponseMsg("scoreboard");

                    if (userRepository.DoesTokenExist(authenticationToken))
                    {
                        response = responseMsg.GetResponseMessage(200) + userRepository.GetAllUserStatsOrderedByElo() + "\r\n";
                    }
                    else response = responseMsg.GetResponseMessage(401);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing JSON: {ex.Message}");
            }
        }

        public string ExtractAuthorizationToken(string request)
        {
            const string authorizationHeader = "Authorization: Bearer ";
            int startIndex = request.IndexOf(authorizationHeader);

            if (startIndex >= 0)
            {
                startIndex += authorizationHeader.Length;
                int endIndex = request.IndexOf("\r\n", startIndex);

                if (endIndex >= 0)
                {
                    return request.Substring(startIndex, endIndex - startIndex);
                }
            }

            return "";
        }

        public string GetUsernameByToken(string token)
        {
            string username = "";
            int indexOfHyphen = token.IndexOf('-');

            if (indexOfHyphen != -1) username = token.Substring(0, indexOfHyphen);
            
            return username;
        }


    }
}
