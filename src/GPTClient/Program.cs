
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
            string configPath = Directory.GetCurrentDirectory() + __CONFIG_PATH;
            var config        = new ConfigurationBuilder() // Initialize configuration builder
                .SetBasePath(configPath)                   //  Set the base path to the current directory
                .AddJsonFile("appsettings.json")           // Add JSON configuration file
                .Build();                                  // Build the configuration
            var          apiKey = config["OpenAI:ApiKey"]; // Retrieve the API key from the json configuration file
            OpenAIClient openAI = new OpenAIClient(apiKey);
            bool         exit   = false;
            string?      question;

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

                if(!exit)
                {
                    var response = openAI.Ask(question);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(response);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();
                }
            }
        }
    }
}