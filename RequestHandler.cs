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
        public string request { get; private set; }

        public string response { get; private set; }

        public RequestHandler(string Request)
        {
            request = Request;

            Handler();
        }

        private void Handler()
        {
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
                            response = responseMsg.GetResponseMessage(200);
                        }
                        else
                        {
                            response = responseMsg.GetResponseMessage(401);
                        }
                    }

                    //3 Punkt bei skript
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error parsing JSON: {ex.Message}");
                }
            }
        }

        
    }
}
