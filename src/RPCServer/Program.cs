﻿using Microsoft.Extensions.Configuration;
using System.Net;
using RPCLibrary.Server;

namespace RPCServerApp
{
    class Program
    {
        record AppSettings
        {
            public string? ApiKey { get; set; }
        }

        private const int     __PORT              = 1999;
        private const string  __RPC_SERVER_APP    = "RPC SERVER";
        private static string __DESTINATION_PATH  = "../../../Resources";
        private static string __APP_SETTINGS_PATH = "../../../../Resources";

        static void Main(string[] args)
        {
            string configPath = Directory.GetCurrentDirectory() + __APP_SETTINGS_PATH;
            var config        = new ConfigurationBuilder()
                .SetBasePath(configPath)
                .AddJsonFile("appsettings.json")
                .Build();
            var apiKey        = config["OpenAI:ApiKey"];
            RPCServer server  = new RPCServer(IPAddress.Any, 1999, __DESTINATION_PATH, apiKey);

            Console.Title = $"{__RPC_SERVER_APP} - {IPAddress.Any}:{__PORT}";

            if (server.Start())
            {
                Console.WriteLine("RPC Server Started");
                server.Run();
                Console.WriteLine("RPC Server Finished");
            }
            else
            {
                Console.WriteLine("Error to start RPC server");
            }
        }
    }
}