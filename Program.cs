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

                    RequestHandler requestHandler = new RequestHandler(request);

                    byte[] resp = Encoding.UTF8.GetBytes(requestHandler.Response);
                    await networkStream.WriteAsync(resp, 0, resp.Length);


                }
            }
        }
    }
}