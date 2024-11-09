using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using HakayaatBilArabiya.Models;
using System.Collections.Generic;

namespace HakayaatBilArabiya.Services
{
    public class WatsonAssistantClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _serviceUrl;
        private readonly string _projectId;
        private readonly string _modelId;
        private string _accessToken;
        private DateTime _tokenExpiration;
        private readonly object _tokenLock = new object();

        public WatsonAssistantClient(HttpClient httpClient, IConfiguration configuration)
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
            lock (_tokenLock)
            {
                if (!string.IsNullOrEmpty(_accessToken) && _tokenExpiration > DateTime.UtcNow)
                {
                    return _accessToken;
                }
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

            lock (_tokenLock)
            {
                _accessToken = responseObject.access_token;
                int expiresIn = responseObject.expires_in;
                _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn - 60);
            }

            return _accessToken;
        }

        public async Task<string> GenerateTextAsync(StoryInputModel inputModel)
        {
            var startingPhrases = new List<string>
            {
                "كان يا ما كان،",
                "في قديم الزمان،",
                "في يوم من الأيام،"
            };

            var random = new Random();
            int index = random.Next(startingPhrases.Count);
            string storyBeginning = startingPhrases[index];

            string inputText;
            if (!string.IsNullOrWhiteSpace(inputModel.ManualInput))
            {
                inputText = $"{storyBeginning} {inputModel.ManualInput}.\n\nاكتب قصة مفصلة وممتعة تبدأ بهذه الجملة.";
            }
            else
            {
                var storyParts = new List<string>();

                if (!string.IsNullOrWhiteSpace(inputModel.Characters))
                    storyParts.Add($"الشخصيات: {inputModel.Characters}");

                if (!string.IsNullOrWhiteSpace(inputModel.Animals))
                    storyParts.Add($"الحيوانات: {inputModel.Animals}");

                if (!string.IsNullOrWhiteSpace(inputModel.Place))
                    storyParts.Add($"المكان: {inputModel.Place}");

                if (!string.IsNullOrWhiteSpace(inputModel.Season))
                    storyParts.Add($"الموسم: {inputModel.Season}");

                if (!string.IsNullOrWhiteSpace(inputModel.Time))
                    storyParts.Add($"الوقت: {inputModel.Time}");

                if (!string.IsNullOrWhiteSpace(inputModel.Style))
                    storyParts.Add($"الأسلوب: {inputModel.Style}");

                if (!string.IsNullOrWhiteSpace(inputModel.MoralValue))
                    storyParts.Add($"القيمة الأخلاقية: {inputModel.MoralValue}");

                string storyDescription = string.Join(", ", storyParts);

                if (string.IsNullOrWhiteSpace(storyDescription))
                {
                    throw new Exception("لم يتم توفير معلومات كافية لتوليد القصة.");
                }

                inputText = $"{storyBeginning} {storyDescription}.\n\nاكتب قصة ممتعة بناءً على المعلومات المذكورة أعلاه.";
            }
            if (inputModel.Style == "درامي مؤثر")
            {
                inputText += "\n\nركز على التفاصيل العاطفية والأحداث المؤثرة لجعل القصة تلامس القلوب بشكل عميق. اجعل النص مليء بالمشاعر القوية التي تبقى في الذاكرة.";
            }
            else if (inputModel.Style == "مشوق ومليء بالمغامرات")
            {
                inputText += "\n\nأضف عناصر مثيرة وأحداث مغامرات تجعل القارئ متشوقاً لمعرفة النهاية.";
            }
            else if (inputModel.Style == "تعليمي بسيط")
            {
                inputText += "\n\nقم بتضمين معلومات مفيدة ودروس قيمة بطريقة مبسطة ومفهومة.";
            }
            else if (inputModel.Style == "فكاهي ومرح")
            {
                inputText += "\n\nاستخدم عناصر الفكاهة والمواقف المرحة لإضفاء جو من البهجة على القصة.";
            }
            else if (inputModel.Style == "خيالي وساحر")
            {
                inputText += "\n\nاستخدم عناصر الخيال والسحر لخلق عوالم جديدة ومشوقة تجذب القارئ.";
            }



            System.Diagnostics.Debug.WriteLine($"Input Text: {inputText}");

            inputText += "\nاكتب قصة ممتعة بناءً على المعلومات المذكورة أعلاه.";

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
                    max_new_tokens = 500,
                    repetition_penalty = 1.1,
                    temperature = 0.7,
                    top_p = 0.9,
                    top_k = 50,
                    stop_sequences = new[] { "الختام" }
                },
                project_id = _projectId
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error from IBM Watson API: {errorResponse}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic responseObject = JsonConvert.DeserializeObject(jsonResponse);

            if (responseObject != null && responseObject.results != null && responseObject.results[0].generated_text != null)
            {
                string generatedText = responseObject.results[0].generated_text.ToString();

                generatedText = RemoveUnwantedPhrases(generatedText);

                return generatedText;
            }
            else
            {
                return "لم يتم توليد أي نص.";
            }
        }

        private string RemoveUnwantedPhrases(string text)
        {
            var unwantedPhrases = new List<string>
            {
                "في هذا السياق، يمكن أن تكون القصة كالتالي:",
                "في هذا السياق يمكن أن تكون القصة كالتالي:",
                "يمكن أن تكون القصة كالتالي:"
            };

            foreach (var phrase in unwantedPhrases)
            {
                text = text.Replace(phrase, "").Trim();
            }

            return text;
        }
    }
}