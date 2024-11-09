using Google.Cloud.TextToSpeech.V1;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.IO;

namespace HakayaatBilArabiya.Services
{
    public class TextToSpeechService
    {
        private readonly TextToSpeechClient _client;

        public TextToSpeechService(IConfiguration configuration)
        {
            var credentialPath = configuration["GoogleCloud:CredentialFilePath"];

            if (string.IsNullOrEmpty(credentialPath) || !File.Exists(credentialPath))
            {
                throw new FileNotFoundException("ملف بيانات اعتماد Google Cloud غير موجود أو لم يتم تحديده في appsettings.json.", credentialPath);
            }

            var clientBuilder = new TextToSpeechClientBuilder
            {
                CredentialsPath = Path.Combine(Directory.GetCurrentDirectory(), credentialPath)
            };
            _client = clientBuilder.Build();
        }

        public async Task<byte[]> SynthesizeAsync(string text)
        {
            var input = new SynthesisInput
            {
                Text = text
            };

            var voice = new VoiceSelectionParams
            {
                LanguageCode = "ar-XA", 
                SsmlGender = SsmlVoiceGender.Female
            };

            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            var response = await _client.SynthesizeSpeechAsync(input, voice, audioConfig);
            return response.AudioContent.ToByteArray();
        }
    }
}
