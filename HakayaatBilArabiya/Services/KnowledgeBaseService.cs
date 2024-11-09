using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace HakayaatBilArabiya.Services
{
    public class KnowledgeBaseService
    {
        private readonly Dictionary<string, string> _animals;
        private readonly Dictionary<string, string> _characters;
        private readonly Dictionary<string, string> _place;
        private readonly Dictionary<string, string> _season;
        private readonly Dictionary<string, string> _time;
        private readonly Dictionary<string, string> _style;
        private readonly Dictionary<string, string> _moralValues;

        public KnowledgeBaseService()
        {
            _animals = LoadData("Data/animals.json");
            _characters = LoadData("Data/characters.json");
            _place = LoadData("Data/place.json");
            _season = LoadData("Data/season.json");
            _time = LoadData("Data/time.json");
            _style = LoadData("Data/style.json");
            _moralValues = LoadData("Data/moralValues.json");
        }

        private Dictionary<string, string> LoadData(string path)
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public string GetAnimalInfo(string animal)
        {
            return GetInfo(_animals, animal);
        }

        public string GetCharacterInfo(string character)
        {
            return GetInfo(_characters, character);
        }

        public string GetPlaceInfo(string place)
        {
            return GetInfo(_place, place);
        }

        public string GetSeasonInfo(string season)
        {
            return GetInfo(_season, season);
        }

        public string GetTimeInfo(string time)
        {
            return GetInfo(_time, time);
        }

        public string GetStyleInfo(string style)
        {
            return GetInfo(_style, style);
        }

        public string GetMoralValueInfo(string moralValue)
        {
            return GetInfo(_moralValues, moralValue);
        }

        private string GetInfo(Dictionary<string, string> data, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;

            var keys = key.Split(new[] { ',', '،' }, StringSplitOptions.RemoveEmptyEntries);
            var infos = new List<string>();

            foreach (var k in keys)
            {
                var trimmedKey = k.Trim();
                if (data.TryGetValue(trimmedKey, out var info))
                {
                    infos.Add(info);
                }
            }

            return string.Join("; ", infos);
        }
    }
}
