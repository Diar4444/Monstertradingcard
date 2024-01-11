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

        public RequestHandler(string Request)
        {
            request = Request;

            Handler();
        }

        private void Handler()
        {
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
                Console.WriteLine("JSON Payload:");
                Console.WriteLine(jsonPayload);

                try
                {
                    if (request.Contains("POST /users")|| request.Contains("POST /sessions"))
                    {
                        // Parse JSON payload
                        var userObject = JsonSerializer.Deserialize<User>(jsonPayload);

                        UserRepository userrep = new UserRepository(userObject);

                        // Check the endpoint and perform specific logic
                        if (request.Contains("POST /users"))
                        {
                            ResponseMsg responseMsg = new ResponseMsg("users");

                            if (!userrep.DoesUserExist())
                            {
                                userrep.AddUser();
                                response = responseMsg.GetResponseMessage(201);
                            }
                            else
                            {
                                response = responseMsg.GetResponseMessage(409);
                            }
                        }
                        else if (request.Contains("POST /sessions"))
                        {
                            ResponseMsg responseMsg = new ResponseMsg("sessions");

                            if (userrep.UserLogin())
                            {
                                response = responseMsg.GetResponseMessage(200) + "Token: " + userObject.Username + "-mtcgToken";
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

                        if(authenticationToken == adminToken)
                        {
                            PackageRepository packageRepository = new PackageRepository();

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
                        else
                        {
                            response = responseMsg.GetResponseMessage(401); 
                        }
                    }

                    else if (request.Contains("POST /transactions/packages"))
                    {
                        //KA wohin aber user braucht geld undsw
                    }

                    //4 Punkt bei skript
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error parsing JSON: {ex.Message}");
                }
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
    }
}
