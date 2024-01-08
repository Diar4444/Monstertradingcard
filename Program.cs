using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MonsterTradingCardGame
{
    internal class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Hello, World!");

            await listenerOption(10001);
        }

        public static async Task listenerOption(int port)
        {

            Console.WriteLine("listening on {0}, port {1}", IPAddress.Any, port);
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            Socket listenerSocket = listener.Server;

            LingerOption lingerOption = new LingerOption(true, 10);
            listenerSocket.SetSocketOption(SocketOptionLevel.Socket,
                              SocketOptionName.Linger,
                              lingerOption);

            // start listening and process connections here.
            listener.Start();

            while (true) {
                using (var client = await listener.AcceptTcpClientAsync()) 
                using (var networkStream = client.GetStream())
                {
                    var requestBytes = new byte[1024];
                    await networkStream.ReadAsync(requestBytes, 0, requestBytes.Length);
                    var request = Encoding.UTF8.GetString(requestBytes);

                    //Console.WriteLine(request.ToString());

                    if (request.Contains("GET / "))
                    {
                        Console.WriteLine("Start");
                        string sendData = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nHello, World!";
                        byte[] resp= Encoding.UTF8.GetBytes(sendData);
                        await networkStream.WriteAsync(resp,0,resp.Length);

                        Console.WriteLine(resp);
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
                        string sendData = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nHello, User!";
                        byte[] resp = Encoding.UTF8.GetBytes(sendData);
                        await networkStream.WriteAsync(resp, 0, resp.Length);
                    }


                }
            }
        }
    }
}