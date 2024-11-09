using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HakayaatBilArabiya.Services
{
    public class AllamSynonymsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _serviceUrl;
        private readonly string _projectId;
        private readonly string _modelId;
        private string _accessToken;
        private DateTime _tokenExpiration;

        public AllamSynonymsService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            _apiKey = configuration["IBMWatson:ApiKey"];
            _serviceUrl = configuration["IBMWatson:ServiceUrl"];
            _projectId = configuration["IBMWatson:ProjectId"];
            _modelId = configuration["IBMWatson:ModelId"];

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_serviceUrl) ||
                string.IsNullOrEmpty(_projectId) || string.IsNullOrEmpty(_modelId))
            {
                throw new ArgumentException("One or more IBM Watson settings are missing in the configuration.");
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiration > DateTime.UtcNow)
            {
                return _accessToken;
            }

            var tokenUrl = "https://iam.cloud.ibm.com/identity/token";
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "urn:ibm:params:oauth:grant-type:apikey"),
                new KeyValuePair<string, string>("apikey", _apiKey)
            });

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = requestBody
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error obtaining access token: {errorResponse}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject(jsonResponse);

            _accessToken = responseObject.access_token;
            int expiresIn = responseObject.expires_in;
            _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn - 60);

            return _accessToken;
        }

        public async Task<List<string>> GetSynonymsAsync(string word)
        {
            var inputText = $"ما هي المرادفات المختلفة لكلمة '{word}' باللغة العربية؟ الرجاء ذكرها بشكل قائمة مفصولة بفواصل.";

            System.Diagnostics.Debug.WriteLine($"Input Text: {inputText}");

            var accessToken = await GetAccessTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var requestBody = new
            {
                model_id = _modelId,
                input = inputText,
                parameters = new
                {
                    decoding_method = "greedy",
                    max_new_tokens = 150,
                    repetition_penalty = 1.1,
                    temperature = 0.5,
                    top_p = 1.0,
                    top_k = 50,
                    stop_sequences = new[] { "\n\n", "النهاية" }
                },
                project_id = _projectId
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"API Response: {jsonResponse}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error from IBM Watson API: {jsonResponse}");
            }

            dynamic responseObject = JsonConvert.DeserializeObject(jsonResponse);

            if (responseObject != null && responseObject.results != null && responseObject.results[0].generated_text != null)
            {
                string generatedText = responseObject.results[0].generated_text.ToString();

                System.Diagnostics.Debug.WriteLine($"Generated Text for word '{word}':");
                System.Diagnostics.Debug.WriteLine(generatedText);

                var synonyms = ExtractSynonymsFromGeneratedText(generatedText, word);

                System.Diagnostics.Debug.WriteLine("Extracted Synonyms:");
                foreach (var synonym in synonyms)
                {
                    System.Diagnostics.Debug.WriteLine(synonym);
                }

                return synonyms;
            }
            else
            {
                return new List<string> { "لم يتم العثور على مرادفات." };
            }
        }

        private List<string> ExtractSynonymsFromGeneratedText(string generatedText, string word)
        {
            var synonymsText = generatedText.Replace($"مرادفات كلمة '{word}':", "").Trim();

            var synonyms = synonymsText.Split(new[] { ',', '،', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(s => s.Trim())
                                       .ToList();

            return synonyms;
        }



        private List<string> ExtractSynonymsFromGeneratedText(string generatedText)
        {
            var parts = generatedText.Split(new[] { ':' }, 2);
            if (parts.Length == 2)
            {
                var synonymsPart = parts[1];
                var synonyms = synonymsPart.Split(new[] { ',', '،' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(s => s.Trim())
                                           .ToList();
                return synonyms;
            }
            else
            {
                return new List<string>();
            }
        }

    }
}
