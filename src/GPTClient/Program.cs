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
using RPCLibrary;

namespace GPTClient
{
    class Program
    {
        private const string __GPT_CLIENT_APP = "GPT CLIENT";
        private static string __CONFIG_PATH = "/../../../Resources";


        static void Main(string[] args)
        {
            try
            {
                string configPath = Directory.GetCurrentDirectory() + __CONFIG_PATH;
                var config = new ConfigurationBuilder()    // Initialize configuration builder
                    .SetBasePath(configPath)                      //  Set the base path to the current directory
                    .AddJsonFile("appsettings.json")              // Add JSON configuration file
                    .Build();                                     // Build the configuration
                var apiKey = config["OpenAI:ApiKey"]; // Retrieve the API key from the json configuration file
                var maxTokens = int.Parse(config["OpenAI:MaxTokens"]);
                OpenAIClient openAI = new OpenAIClient(apiKey, maxTokens);
                bool exit = false;
                string? question;

                Console.Title = $"{__GPT_CLIENT_APP}";

                while (!exit)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Ask GPT");
                    Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine();
                    Console.Write("==> ");

                    question = Console.ReadLine();  // Read user question
                    exit = ((question != null) && (question?.Length == 0));

                    if (!exit)
                    {
                        var response = openAI.Ask(question);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(response);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine();
                    }
                }
            } catch(Exception e)
            {
                Console.WriteLine($"Error: {e}");
            }
        }
    }
}