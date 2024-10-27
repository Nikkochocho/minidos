/*
 * MiniDOS
 * Copyright (C) 2024  Lara H. Ferreira and others.
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Microsoft.Extensions.Configuration;
using System.Net;
using RPCLibrary.Server;
using RPCLibrary;
using RPCLibrary.Config;

namespace RPCServerApp
{
    class Program
    {
        record AppSettings
        {
            public string? ApiKey { get; set; }
        }

        private const int     __WINDOW_HEIGHT     = 33;
        private const int     __PORT              = 1999;
        private const string  __RPC_SERVER_APP    = "RPC SERVER";
        private static string __APP_SETTINGS_PATH = "../../../../Resources";

        static void Main(string[] args)
        {
            try
            {
                string configPath = Directory.GetCurrentDirectory() + __APP_SETTINGS_PATH;
                var config = new ConfigurationBuilder()
                    .SetBasePath(configPath)
                    .AddJsonFile("appsettings.json")
                    .Build();
                ServerParms parms = new ServerParms()
                {
                    ApiKey         = config["OpenAI:ApiKey"],
                    MaxTokens      = int.Parse(config["OpenAI:MaxTokens"]),
                    SharedFolder   = config["SharedFolder"],
                    DownloadFolder = config["DownloadFolder"],
                    ShowScreenContentOnServer = bool.Parse(config["ShowScreenContentOnServer"]),
                };
                RPCServer server = new RPCServer(IPAddress.Any, 1999, parms);

                Console.Title        = $"{__RPC_SERVER_APP} - {IPAddress.Any}:{__PORT}";
                Console.WindowHeight = __WINDOW_HEIGHT;

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
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }
    }
}