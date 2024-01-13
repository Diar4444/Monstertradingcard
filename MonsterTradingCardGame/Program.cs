using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using MonsterTradingCardGame.Repository;

namespace MonsterTradingCardGame
{
    internal class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Server Started!");

            await listenerOption(10001);
        }

        public static async Task listenerOption(int port)
        {
            Console.WriteLine("listening on {0}, port {1}", IPAddress.Any, port);
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            Socket listenerSocket = listener.Server;

            LingerOption lingerOption = new LingerOption(true, 10);
            listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);

            Console.Write("Do you want to init(reset) the DB?(Y or N)");
            char resetDB = Console.ReadKey().KeyChar;

            if (resetDB == 'Y')
            {
                DBinitRepository dBinitRepository = new DBinitRepository();
            }
            Console.WriteLine();

            listener.Start();

            while (true)
            {
                using (var client = await listener.AcceptTcpClientAsync())
                {
                    _ = ProcessRequestAsync(client);
                }
            }
        }

        private static async Task ProcessRequestAsync(TcpClient client)
        {
            using (var networkStream = client.GetStream())
            {
                var requestBytes = new byte[1024];
                await networkStream.ReadAsync(requestBytes, 0, requestBytes.Length);
                var request = Encoding.UTF8.GetString(requestBytes);

                RequestHandler requestHandler = new RequestHandler(request);

                byte[] resp = Encoding.UTF8.GetBytes(requestHandler.response);
                await networkStream.WriteAsync(resp, 0, resp.Length);
            }
        }
    }
}