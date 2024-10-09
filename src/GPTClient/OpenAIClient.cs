using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration; //required
using Newtonsoft.Json; //requied


namespace MiniDOS.GPTClient
{ 
    internal class GptClient
    {
        private readonly HttpClient _httpClient;  // HTTP client for sending requests
        private readonly string _apiKey; //// OpenAI API key

        public GptClient()
        {
            var config = new ConfigurationBuilder() // Initialize configuration builder
                .SetBasePath(Directory.GetCurrentDirectory()) //  Set the base path to the current directory
                .AddJsonFile("appsettings.json") // Add JSON configuration file
                .Build(); // Build the configuration

            _apiKey = config["OpenAI:ApiKey"]; // Retrieve the API key from the json configuration file

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey); // Set the Authorization header with the API key
        }

        public async Task RunAsync()
        {
            while (true) // Infinite loop to continuously accept user input
            { 
        
                var question = Console.ReadLine();  // Read user question

                if (string.IsNullOrWhiteSpace(question)) // Check if the input is empty or whitespace
                {
                    Console.WriteLine("Você deve fazer uma pergunta...");
                }
                else
                {
                    // Serialize the request payload to JSON format
                    var json = JsonConvert.SerializeObject(new
                    {
                        model = "gpt-3.5-turbo-instruct", // Specify the model to use
                        prompt = question, // Set the user's question as the prompt
                        max_tokens = 300, // Limit the response to 300 tokens
                        temperature = 1 // Set the randomness of the response
                    });

                    // Send a POST request to the OpenAI API
                    var httpResponse = await _httpClient.PostAsync(
                        "https://api.openai.com/v1/completions", // OpenAI API endpoint
                        new StringContent(json, Encoding.UTF8, "application/json") // Request content and headers
                    );

                    // Read the response content as a string
                    var data = await httpResponse.Content.ReadAsStringAsync();

                    // Deserialize the response JSON into a dynamic object
                    var response = JsonConvert.DeserializeObject<dynamic>(data);

                    // Check if the response contains valid choices
                    if (response?.choices != null && response.choices.Count > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(response.choices[0].text); // Write the response received
                        Console.WriteLine();
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine("Resposta não contém escolhas válidas.");
                    }
                }
            }
        }
    }
}