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
            if(path == "users")
            {
                messages = new Dictionary<int, string>
                {
                    { 201, "HTTP/1.1 201 Created\r\nContent-Type: text/plain\r\n\r\nUser successfully created\r\n" },
                    { 409, "HTTP/1.1 409 Conflict\r\nContent-Type: text/plain\r\n\r\nUser with same username already registered\r\n" },
                };
            }
            else if(path == "sessions")
            {
                messages = new Dictionary<int, string>
                {
                    { 200, "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\nUser login successful\r\n" },
                    { 401, "HTTP/1.1 401 Unauthorized\r\nContent-Type: text/plain\r\n\r\nInvalid username/password provided\r\n" },
                };
            }
            else if(path == "packages")
            {
                messages = new Dictionary<int, string>
                {
                    { 201, "HTTP/1.1 201 Created\r\nContent-Type: text/plain\r\n\r\nPackage and cards successfully created\r\n" },
                    { 401, "HTTP/1.1 401 Created\r\nContent-Type: text/plain\r\n\r\nAccess token is missing or invalid\r\n" },
                    { 403, "HTTP/1.1 403 Created\r\nContent-Type: text/plain\r\n\r\nProvided user is not \"admin\"\r\n" },
                    { 409, "HTTP/1.1 409 Conflict\r\nContent-Type: text/plain\r\n\r\nAt least one card in the packages already exists\r\n" },
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
