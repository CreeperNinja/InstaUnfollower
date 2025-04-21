using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Main.Launches
{
    internal class ProgramData : SaveData
    {
        public ProgramData() : base("Program.json") { }

        public ProgramData(string fileName) : base(fileName) { }

        public DateTime LastUpdated { get; set; }
        public int Unfollowed {  get; set; }

        public const int dailyUnfollowLimit = 150;
        DateTime nextUnfollowTime;
        TimeSpan remainingTime;

        public override void Save()
        {
            LastUpdated = DateTime.Now;

            Directory.CreateDirectory(AppFolder); // Ensures folder exists

            // Convert to JSON
            string json = JsonSerializer.Serialize(this, jsonOptions);

            // Save
            File.WriteAllText(FilePath, json);
        }

        public bool Load()
        {
            if (File.Exists(FilePath))
            {
                string jsonFromFile = File.ReadAllText(FilePath);
                var temp = JsonSerializer.Deserialize<ProgramData>(jsonFromFile);
                if (temp != null)
                {
                    LastUpdated = temp.LastUpdated;
                    Unfollowed = temp.Unfollowed;
                    return true;
                }
            }

            LastUpdated = DateTime.Now;
            Unfollowed = 0;
            return false;
        }

        public bool ReachedDailyLimit()
        {
            if (Unfollowed >= dailyUnfollowLimit)
            {
                Console.WriteLine($"\r\nDaily Limit Of {ProgramData.dailyUnfollowLimit} Unfollows Reached\r\n");
                return true;
            }
            return false;
        }

        public bool IsNextDay()
        {
            return (DateTime.Now - LastUpdated).TotalDays >= 1;
        }

        public void WriteRemainingTime()
        {
            nextUnfollowTime = LastUpdated.AddHours(24);
            remainingTime = nextUnfollowTime - DateTime.Now;

            List<string> parts = [];
            if (remainingTime.Hours > 0) parts.Add($"{remainingTime.Hours}h");
            if (remainingTime.Minutes > 0) parts.Add($"{remainingTime.Minutes}m");
            string formatted = string.Join(" ", parts);

            Console.Write($"\r---Next Allowd Time Is {nextUnfollowTime} (In {formatted})---");
        }

        public bool CheckIfCanUnfollow()
        {
            //If A Full Day Has Passed Reset Unfollowed Count
            if (IsNextDay())
            {
                Unfollowed = 0;
                Save();
                return true;
            }

            //Return False If Daily Unfollow Was Reached
            if (ReachedDailyLimit())
            {
                return false; 
            }

            return true;
        }
    }
}
