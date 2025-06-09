using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
            List<string> nonNullOrEmptyEntries = entries.Where(s => !string.IsNullOrEmpty(s)).ToList();
            Debug.WriteLine("Lock n loaded");
            LoadData();
            if (TestData.ContainsKey(title))
            {
                TestData[title] = nonNullOrEmptyEntries;
                Debug.WriteLine($"Update existing: {title}");
                entries.ForEach(t => Debug.WriteLine(t));
            }
            else
            {
                TestData.Add(title, nonNullOrEmptyEntries);
                Debug.WriteLine($"Add new: {title}");
                entries.ForEach(t => Debug.WriteLine(t));
            }
            SaveData();
        }
    }
}