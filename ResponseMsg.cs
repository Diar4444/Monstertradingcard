using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    public class ResponseMsg
    {
        private readonly Dictionary<int,string> messages;

        public ResponseMsg(string path)
        {
            if (path == "users")
            {
                messages = new Dictionary<int, string>
                {
                    { 201, "HTTP/1.1 201 Created\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User successfully created\"}\r\n" },
                    { 409, "HTTP/1.1 409 Conflict\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User with same username already registered\"}\r\n" },
                };
            }
            else if (path == "sessions")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User login successful\"}\r\n" },
                    { 401, "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Invalid username/password provided\"}\r\n" },
                };
            }
            else if (path == "packages")
            {
                messages = new Dictionary<int, string>
                {
                    { 201, "HTTP/1.1 201 Created\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Package and cards successfully created\"}\r\n" },
                    { 401, "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" },
                    { 403, "HTTP/1.1 403 Forbidden\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Provided user is not \"admin\"\"}\r\n" },
                    { 409, "HTTP/1.1 409 Conflict\r\nContent-Type: application/json\r\n\r\n{\"message\": \"At least one card in the packages already exists\"}\r\n" },
                };
            }
            else if (path == "transactions/packages")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"A package has been successfully bought\"}\r\n" },
                    { 401, "HTTP/1.1 401 Unauthorized\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" },
                    { 403, "HTTP/1.1 403 No Money\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Not enough money for buying a card package\"}\r\n" },
                    { 404, "HTTP/1.1 404 No Package\r\nContent-Type: application/json\r\n\r\n{\"message\": \"No card package available for buying\"}\r\n" },
                };
            }
            else if (path == "cards")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The user has cards, the response contains these\"}\r\n" },
                    { 204, "HTTP/1.1 204 No Card\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The request was fine, but the user doesn't have any cards\"}\r\n" },
                    { 401, "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" }
                };
            }
            else if (path == "deckGET")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The deck has cards, the response contains these\"}\r\n" },
                    { 204, "HTTP/1.1 204 No Card\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The request was fine, but the deck doesn't have any cards\"}\r\n" },
                    { 401, "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" }
                };
            }
            else if (path == "deckPUT")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The deck has been successfully configured\"}\r\n" },
                    { 400, "HTTP/1.1 400 Amount Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The provided deck did not include the required amount of cards\"}\r\n" },
                    { 401, "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" },
                    { 403, "HTTP/1.1 403 Not Your Card\r\nContent-Type: application/json\r\n\r\n{\"message\": \"At least one of the provided cards does not belong to the user or is not available.\"}\r\n" }
                };
            }
            else if (path == "userGET")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Data successfully retrieved\"}\r\n" },
                    { 401, "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" },
                    { 404, "HTTP/1.1 404 Not Found\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User not found.\"}\r\n" }
                };
            }
            else if (path == "userPUT")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User sucessfully updated.\"}\r\n" },
                    { 401, "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" },
                    { 404, "HTTP/1.1 404 Not Found\r\nContent-Type: application/json\r\n\r\n{\"message\": \"User not found.\"}\r\n" }
                };
            }
            else if (path == "stats")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The stats could be retrieved successfully.\"}\r\n" },
                    { 401, "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" }
                };
            }
            else if (path == "scoreboard")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The scoreboard could be retrieved successfully.\"}\r\n" },
                    { 401, "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" }
                };
            }
            else if (path == "battle")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n\r\n{\"message\": \"The battle has been carried out successfully.\"}\r\n" },
                    { 401, "HTTP/1.1 401 Token Error\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Access token is missing or invalid\"}\r\n" },
                    { 402, "HTTP/1.1 402 same user\r\nContent-Type: application/json\r\n\r\n{\"message\": \"Same User registered twice\"}\r\n" }

                };
            }
        }

        public string GetResponseMessage(int statusCode)
        {
            if (messages.TryGetValue(statusCode, out string message))
            {
                return message;
            }

            return $"HTTP/1.1 {statusCode} \r\nContent-Type: text/plain\r\n\r\nUnknown Status Code";
        }
    }
}
