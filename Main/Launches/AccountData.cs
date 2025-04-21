using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Main.Launches
{
    internal class AccountData : SaveData
    {
        //Account
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public HashSet<string> Followers { get; set; } = [];

        public AccountData() : base("Account.json") { }

        public AccountData(string fileName) : base(fileName) { }

        public override void Save()
        {
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
                var temp = JsonSerializer.Deserialize<AccountData>(jsonFromFile);
                if (temp != null)
                {
                    Name = temp.Name;
                    Username = temp.Username;
                    Password = temp.Password;
                    Followers = temp.Followers;

                    Console.WriteLine($"\r--Loaded {Name}'s Profile with {Followers.Count} Followers--");
                    return true;
                }
            }
            return false;
        }

    }
}
