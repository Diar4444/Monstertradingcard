using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    public class RequestHandler
    {
        public string Request { get; private set; }

        public string Response { get; private set; }

        public RequestHandler(string request)
        {
            Request = request;

            if (request.Contains("GET / "))
            {
                Console.WriteLine("Start");
                Response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nHello, World!";

                string connectionString = "Host=localhost;Username=postgres;Password=Halamadrid1;Database=postgres";

                UserRepository userRepository = new UserRepository(connectionString);

                List<User> users = userRepository.GetAllUsers();

                Console.WriteLine("All Users:");
                foreach (User user in users)
                {
                    Console.WriteLine(user.Username);
                }
            }

            if (request.Contains("POST /users"))
            {
                Console.WriteLine("Start");

                // Extract JSON payload from request
                int bodyStartIndex = request.IndexOf("{");
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
                        // Parse JSON payload
                        var userObject = JsonSerializer.Deserialize<User>(jsonPayload);

                        // Access username and password
                        Console.WriteLine($"Username: {userObject.Username}, Password: {userObject.Password}");
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Error parsing JSON: {ex.Message}");
                    }
                }

                // Respond to the client
                Response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nHello, User!";
            }
        }

        
    }
}
