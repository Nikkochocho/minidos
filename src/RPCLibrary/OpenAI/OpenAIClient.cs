using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json; //requied


namespace RPCLibrary
{
    public class OpenAIClient
    {
        private const int __DEFAULT_MAX_TOKENS = 2048;

        private struct GPTPayload    // Chat GPT payload
        {
            public string model { get; set; }
            public string prompt { get; set; }
            public int max_tokens { get; set; }
            public int temperature { get; set; }
        }

        private readonly HttpClient _httpClient;   // HTTP client for sending requests
        private readonly string     _apiKey;       // OpenAI API key

        public int MaxTokens { get; set; } = __DEFAULT_MAX_TOKENS; // Response Max chars

        public OpenAIClient(string apiKey, int maxTokens)
        {
            _apiKey     = apiKey;
            _httpClient = new HttpClient();
            MaxTokens   = (maxTokens == 0 ? __DEFAULT_MAX_TOKENS : maxTokens);

            // Set the Authorization header with the API key
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public string Ask(string question)
        {
            GPTPayload payload = new GPTPayload
            {
                model = "gpt-3.5-turbo-instruct", // Specify the model to use
                prompt = question,                // Set the user's question as the prompt
                max_tokens = MaxTokens,           // Limit the response to MaxTokens property
                temperature = 1                   // Set the randomness of the response
            };

            // Serialize the request payload to JSON format
            var json = JsonConvert.SerializeObject(payload);

            // Send a POST request to the OpenAI API
            var httpResponse = _httpClient.PostAsync( 
                "https://api.openai.com/v1/completions",                   // OpenAI API endpoint
                new StringContent(json, Encoding.UTF8, "application/json") // Request content and headers
            );

            // Read the response content as a string
            var data = httpResponse.Result.Content.ReadAsStringAsync();

            // Deserialize the response JSON into a dynamic object
            var response = JsonConvert.DeserializeObject<dynamic>(data.Result);

            // Check if the response contains valid choices
            if (response?.choices != null && response.choices.Count > 0)
            {
                return response.choices[0].text; // Return the response received
            }

            return "Response with no valid choices";
        }
    }
}