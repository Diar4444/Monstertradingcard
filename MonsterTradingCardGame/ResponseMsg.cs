﻿using System;
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