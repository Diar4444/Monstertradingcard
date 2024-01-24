using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using MonsterTradingCardGame.Repository;
using System.Linq;

namespace MonsterTradingCardGame
{
    internal class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Server Started!");

            await ListenerOption(10001);
        }

        public static async Task ListenerOption(int port)
        {
            Console.WriteLine($"Listening on {IPAddress.Any}, port {port}");
            TcpListener listener = new TcpListener(IPAddress.Any, port);

            LingerOption lingerOption = new LingerOption(true, 10);
            listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);

            Console.Write("Do you want to init(reset) the DB? (Y or N)");
            char resetDB = Console.ReadKey().KeyChar;

            if (resetDB == 'Y')
            {
                DBinitRepository dBinitRepository = new DBinitRepository();
            }
            Console.WriteLine();

            listener.Start();

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();

                _ = Task.Run(() => ProcessRequestAsync(client));
            }
        }

        private static async Task ProcessRequestAsync(TcpClient client)
        {
            try
            {
                using (var networkStream = client.GetStream())
                {
                    var requestBytes = new byte[1024];
                    int bytesRead = await networkStream.ReadAsync(requestBytes, 0, requestBytes.Length);
                    byte[] resp;

                    if (bytesRead > 0)
                    {
                        var request = Encoding.UTF8.GetString(requestBytes, 0, bytesRead);

                        if (request.Contains("POST /battles"))
                        {
                            resp = await BattleRepository.ProcessBattlesRequestAsync(request);
                        }
                        else
                        {
                            RequestHandler requestHandler = new RequestHandler(request);

                            Console.WriteLine("Request: " + request);
                            for (int a = 0; a < 50; a++) Console.Write("_");
                            Console.WriteLine();
                            Console.WriteLine("Response: " + requestHandler.response);
                            for (int b = 0; b < 50; b++) Console.Write("_");
                            Console.WriteLine();

                            resp = Encoding.UTF8.GetBytes(requestHandler.response);
                        }
                        
                        await networkStream.WriteAsync(resp, 0, resp.Length);
                        // Close the client after processing the request
                        client.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
            }
        }
    }
}