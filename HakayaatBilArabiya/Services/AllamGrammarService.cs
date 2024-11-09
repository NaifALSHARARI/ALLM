using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace HakayaatBilArabiya.Services
{
    public class AllamGrammarService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _serviceUrl;
        private readonly string _projectId;
        private readonly string _modelId;
        private string _accessToken;
        private DateTime _tokenExpiration;
        private readonly object _tokenLock = new object();

        public AllamGrammarService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            _apiKey = configuration["IBMWatson:ApiKey"];
            _serviceUrl = configuration["IBMWatson:ServiceUrl"];
            _projectId = configuration["IBMWatson:ProjectId"];
            _modelId = configuration["IBMWatson:ModelId"];

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_serviceUrl) ||
                string.IsNullOrEmpty(_projectId) || string.IsNullOrEmpty(_modelId))
            {
                throw new ArgumentException("إعدادات IBM Watson مفقودة في التكوين.");
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
                throw new Exception($"خطأ في الحصول على رمز الوصول: {errorResponse}");
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

        public string RetrieveRelevantInformation(string topic)
        {
            var knowledgeBase = new Dictionary<string, string>
            {
                { "الفرق بين المذكر والمؤنث في الجمل", "في اللغة العربية، الكلمات تنقسم إلى مذكر ومؤنث. المذكر مثل: ولد، كتاب. المؤنث مثل: بنت، مدرسة. مثال: الولد يقرأ، البنت تقرأ." },
                { "استخدام الأفعال في الزمن الماضي والمضارع والمستقبل", "الأفعال تصرف حسب الزمن: الماضي (ذهب)، المضارع (يذهب)، المستقبل (سيذهب). مثال: ذهب أحمد إلى السوق، يذهب أحمد إلى المدرسة، سيذهب أحمد إلى الحديقة." },
                { "الضمائر المنفصلة والمتصلة", "الضمائر المنفصلة: أنا، أنت، هو، هي، نحن، أنتم، هم. الضمائر المتصلة: -ي، -ك، -ه، -ها، -نا، -كم، -هم. مثال: كتابي، كتابك، كتابه." }
            };

            if (knowledgeBase.ContainsKey(topic))
            {
                return knowledgeBase[topic];
            }
            else
            {
                return "";
            }
        }

        public async Task<string> GenerateGrammarStoryAsync(string topic)
        {
            string retrievedInfo = RetrieveRelevantInformation(topic);

            var prompts = new List<string>
    {
        $"اكتب قصة قصيرة تحتوي على العديد من الأمثلة التي توضح {topic}. ركز على تضمين الأمثلة بشكل واضح ومباشر داخل القصة.",
        $"تخيل حوارًا يدور بين شخصين يستخدمون أمثلة حول {topic}، وركز على تقديم معلومات تعليمية خلال القصة.",
        $"اكتب مشهدًا قصيرًا يدور حول مغامرة تحدث في إطار استخدام {topic} في القصة.",
        $"قدم قصة توضح {topic} بشكل مبتكر وتحتوي على تفاعل بين الشخصيات باستخدام هذه الأمثلة."
    };

            Random rand = new Random();
            string selectedPrompt = prompts[rand.Next(prompts.Count)];
            string prompt = $"{selectedPrompt}\n\n{retrievedInfo}\n\nالقصة:";

            var accessToken = await GetAccessTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, _serviceUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var requestBody = new
            {
                model_id = _modelId,
                input = prompt,
                parameters = new
                {
                    decoding_method = "greedy",
                    max_new_tokens = 500,
                    repetition_penalty = 1.2,
                    temperature = 0.7, 
                    top_p = 0.95, 
                    top_k = 50,
                    stop_sequences = new[] { "النهاية" }
                },
                project_id = _projectId
            };

            string requestBodyJson = JsonConvert.SerializeObject(requestBody);
            request.Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"خطأ من IBM Watson API: {jsonResponse}");
            }

            dynamic responseObject = JsonConvert.DeserializeObject(jsonResponse);

            if (responseObject != null && responseObject.results != null && responseObject.results[0].generated_text != null)
            {
                string generatedText = responseObject.results[0].generated_text.ToString().Trim();

                if (topic.Contains("المذكر والمؤنث"))
                {
                    generatedText = HighlightMasculineAndFeminine(generatedText);
                }
                else if (topic.Contains("الأفعال في الزمن الماضي والمضارع والمستقبل"))
                {
                    generatedText = HighlightTenses(generatedText);
                }
                else if (topic.Contains("الضمائر"))
                {
                    generatedText = HighlightPronouns(generatedText);
                }

                return generatedText;
            }
            else
            {
                return "لم يتم توليد أي نص.";
            }
        }


        public List<Dictionary<string, string>> AnalyzeTextWithPython(string text)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = "python";

            var scriptDirectory = Path.Combine(Directory.GetCurrentDirectory(), "PythonScripts");
            var script = Path.Combine(scriptDirectory, "arabic_gender_analyzer.py");

            var tempInputFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.txt");
            File.WriteAllText(tempInputFile, text, Encoding.UTF8);

            psi.Arguments = $"\"{script}\" \"{tempInputFile}\"";

            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.StandardErrorEncoding = Encoding.UTF8;

            var process = Process.Start(psi);

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            File.Delete(tempInputFile);

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception($"خطأ في سكريبت Python: {error}");
            }

            var analysis = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(output);

            return analysis;
        }

        public string HighlightMasculineAndFeminine(string text)
        {
            var analysis = AnalyzeTextWithPython(text);
            var highlightedText = new StringBuilder();

            foreach (var wordInfo in analysis)
            {
                var word = wordInfo["word"];
                if (wordInfo.ContainsKey("gender"))
                {
                    var gender = wordInfo["gender"];
                    if (gender == "masculine")
                    {
                        highlightedText.Append($"<span style=\"color:blue;\">{word}</span> ");
                    }
                    else if (gender == "feminine")
                    {
                        highlightedText.Append($"<span style=\"color:purple;\">{word}</span> ");
                    }
                    else
                    {
                        highlightedText.Append($"{word} ");
                    }
                }
                else
                {
                    highlightedText.Append($"{word} ");
                }
            }

            return highlightedText.ToString().Trim();
        }

        public string HighlightTenses(string text)
        {
            var analysis = AnalyzeTextWithPython(text);
            var highlightedText = new StringBuilder();

            foreach (var wordInfo in analysis)
            {
                var word = wordInfo["word"];
                if (wordInfo.ContainsKey("pos") && wordInfo["pos"] == "verb")
                {
                    var tense = wordInfo["tense"];
                    if (tense == "past")
                    {
                        highlightedText.Append($"<span style=\"color:green;\">{word}</span> ");
                    }
                    else if (tense == "present")
                    {
                        highlightedText.Append($"<span style=\"color:orange;\">{word}</span> ");
                    }
                    else if (tense == "future")
                    {
                        highlightedText.Append($"<span style=\"color:red;\">{word}</span> ");
                    }
                    else
                    {
                        highlightedText.Append($"{word} ");
                    }
                }
                else
                {
                    highlightedText.Append($"{word} ");
                }
            }

            return highlightedText.ToString().Trim();
        }

        public string HighlightPronouns(string text)
        {
            var analysis = AnalyzeTextWithPython(text);
            var highlightedText = new StringBuilder();

            foreach (var wordInfo in analysis)
            {
                var word = wordInfo["word"];
                if (wordInfo.ContainsKey("pronoun") && wordInfo["pronoun"] == "True")
                {
                    highlightedText.Append($"<span style=\"background-color:yellow;\">{word}</span> ");
                }
                else
                {
                    highlightedText.Append($"{word} ");
                }
            }

            return highlightedText.ToString().Trim();
        }
    }
}
