using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
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

        private ResponseMsg responseMsgUser = new ResponseMsg("users");
        private ResponseMsg responseMsgSession = new ResponseMsg("sessions");
        private ResponseMsg responseMsgPackages = new ResponseMsg("packages");
        private ResponseMsg responseMsgBuy = new ResponseMsg("transactions/packages");



        public RequestHandler(string Request)
        {
            request = Request;

            Handler();
        }

        private void Handler()
        {
            try {
                string authenticationToken = ExtractAuthorizationToken(request);
                int bodyStartIndex = request.IndexOf("{");

                if (request.Contains("POST /packages")) bodyStartIndex = request.IndexOf("[");

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

                            if (!userrep.DoesUserExist())
                            {
                                userrep.AddUser();
                                response = responseMsgUser.GetResponseMessage(201);
                            }
                            else response = responseMsgUser.GetResponseMessage(409);
                        }
                        else if (request.Contains("POST /sessions"))
                        {
                            if (userrep.UserLogin())
                            {
                                response = responseMsgSession.GetResponseMessage(200) + "Token: " + userObject.Username + "-mtcgToken";
                            }
                            else
                            {
                                response = responseMsgSession.GetResponseMessage(401);
                            }
                        }
                    }
                    else if (request.Contains("POST /packages"))
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

                            response = responseMsgPackages.GetResponseMessage(201);
                        }
                        else response = responseMsgPackages.GetResponseMessage(401);
                    }
                }
                else if (request.Contains("POST /transactions/packages"))
                {
                    List<int> packagelist = new List<int>();
                    int coins;
                    string username = GetUsername(authenticationToken);

                    //Ersatz weil das mit token im curl script komisch ist such ich nach username direkt

                    coins = userRepository.GetCoins(username);

                    packagelist = packageRepository.IsPackageAvailable();

                    if (packagelist.Count > 0)
                    {
                        if (coins >= 5)
                        {
                            userRepository.UpdateCoins(coins - 5, username);

                            packageRepository.BuyPackage(packagelist[0], username);

                            response = responseMsgBuy.GetResponseMessage(200);
                        }
                        else
                        {
                            response = responseMsgBuy.GetResponseMessage(403);
                        }
                    }
                    else
                    {
                        response = responseMsgBuy.GetResponseMessage(404);
                    }
                }
                else if (request.Contains("GET /cards"))
                {
                    Console.WriteLine(GetUsername(authenticationToken));
                }

            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing JSON: {ex.Message}");
            }
        }

        private string ExtractAuthorizationToken(string request)
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

            return null;
        }

        private string GetUsername(string token)
        {
            string username = "";

            try
            {
                int indexOfHyphen = token.IndexOf('-');
                if (indexOfHyphen != -1) username = token.Substring(0, indexOfHyphen);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
            return username;
        }
    }
}
