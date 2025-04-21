using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Main.Launches
{
    abstract class SaveData
    {
        //Files
        public static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string AppFolder = Path.Combine(AppDataPath, "InstaUnfollower");
        public readonly string FilePath;

        public static JsonSerializerOptions jsonOptions = new() { WriteIndented = true};

        public SaveData(string fileName)
        {
            FilePath = Path.Combine(AppFolder, fileName);
        }

        public abstract void Save();

        public static void Save(AccountData save, string FilePath)
        {
            Directory.CreateDirectory(AppFolder); // Ensures folder exists

            // Convert to JSON
            string json = JsonSerializer.Serialize(save, jsonOptions);

            // Save
            File.WriteAllText(FilePath, json);
        }

        public static bool Load(ref AccountData save, string FilePath)
        {
            if (File.Exists(FilePath))
            {
                string jsonFromFile = File.ReadAllText(FilePath);
                var temp = JsonSerializer.Deserialize<AccountData>(jsonFromFile);
                if (temp != null)
                {
                    save = temp;
                    Console.WriteLine($"\r--Loaded {save.Name}'s Profile with {save.Followers.Count} Followers--");
                    return true;
                }
            }

            save = new AccountData();
            return false;
        }


    }
}
