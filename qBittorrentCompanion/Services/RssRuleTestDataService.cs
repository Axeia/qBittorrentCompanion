using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace qBittorrentCompanion.Services
{
    public static class RssRuleTestDataService
    {
        private static string FilePath = "RssTestData.json";
        public static Dictionary<string, List<string>> TestData { get; private set; } = [];

        public static void LoadData()
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                TestData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json)!;
            }
        }

        public static void SaveData()
        {
            string json = JsonConvert.SerializeObject(TestData);
            File.WriteAllText(FilePath, json);
        }

        public static List<string> GetEntry(string title)
        {
            LoadData();
            return TestData.GetValueOrDefault(title, []);
        }


        public static void SetValue(string title, List<string> entries)
        {
            LoadData();
            if (TestData.ContainsKey(title))
            {
                TestData[title] = entries;
            }
            else
            {
                TestData.Add(title, entries);
            }
            SaveData();
        }
    }
}