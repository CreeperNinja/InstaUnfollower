using System;
using System.Diagnostics;
using Main.Launches;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Main
{
    internal class Program
    {
        static string username = "vladiplaystest@gmail.com";
        static string password = "IGVG$21$";

        static TabbedLauncher? launcher;

        //Data
        public static AccountData accountData = new();
        public static ProgramData programData = new();

        static void Main(string[] args)
        {

            Console.WriteLine($"\r\n--Note--\r\n Account Data is stored in {SaveData.AppFolder} \r\n");

            //LoadSave
            if (!accountData.Load() || accountData.Username != string.Empty && accountData.Password != string.Empty) AskInput();
            else if (Ask("Do You Want To Reset Followers Data? (y): ", true) == "y") accountData.Followers.Clear();
            programData.Load();

            if (!programData.CheckIfCanUnfollow())
            {
                while (!programData.IsNextDay())
                {
                    programData.WriteRemainingTime();
                    Thread.Sleep(60000);
                }
            }

            launcher = new TabbedLauncher();

            Thread.Sleep(5000);

            launcher?.Start();

        }

        public static void ResumeCountDown(int seconds)
        {
            while (seconds > 0)
            {
                Console.Write($"\rResuming In {seconds} seconds        ");
                Thread.Sleep(1000);
                seconds--;
            }
            Console.Write($"\rResuming In {seconds} seconds        ");
        }

        public static void CountDownQuitTime(ChromeDriver driver, int seconds)
        {
            while (seconds > 0)
            {
                Console.Write($"\rClosing Program In {seconds} seconds        ");
                Thread.Sleep(1000);
                seconds--;
            }

            Console.Write($"\rClosing Program In {seconds} seconds        ");
            driver.Quit();

        }

        static void AskInput()
        {
            //Account
            Ask("\r\nUsername/Email:", ref username);
            Ask("\r\nPassword:", ref password);

            accountData.Username = username;
            accountData.Password = password;

            //Setup
            //AskOperationMode();
        }

        public static ChromeDriver LaunchChromeWithDebugging()
        {
            //var userDataDir = @"C:\ChromeDebug"; // any custom user profile folder
            //var arguments = $"--remote-debugging-port=9222 " +
            //    $"--user-data-dir=\"{userDataDir}\" " +
            //    "--new-window --disable-popup-blocking --auto-open-devtools-for-tabs";

            var options = new ChromeOptions();
            //options.DebuggerAddress = "127.0.0.1:9222"; // Connect to existing Chrome
            options.AddArgument("--start-maximized");
            options.AddArgument(@"--user-data-dir=C:\ChromeDebug");
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--auto-open-devtools-for-tabs");
            options.AddArgument("--new-window");
            options.AddArgument("--remote-debugging-port=9222");

            var driver = new ChromeDriver(options);
            return driver;
        }

        public static void SendKey(IWebDriver driver, string key, params string[] strings)
        {
            if (driver == null) return;

            foreach(var str in strings)
            {
                try
                {
                    var element = driver.FindElement(By.Name(str));
                    if (element != null)
                    {
                        Console.WriteLine($"Succesfully Found Element \"{str}\"");
                        element.SendKeys(key);
                        return;
                    }
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine($"Element for key \"{key}\" was not found");
                }
            }
        }

        static void Ask(string message, ref string data)
        {
            while (true)
            {
                Console.WriteLine(message);
                var text = Console.ReadLine();
                if (text != null && text != "")
                {
                    data = text;
                    return;
                }
                Console.WriteLine("Try Again");
            }
        }

        static string Ask(string message, bool allowEmpty = false)
        {
            while (true)
            {
                Console.WriteLine(message);
                var text = Console.ReadLine();
                if (allowEmpty || text != null && text != "")
                {
                    return text;
                }
                Console.WriteLine("Try Again");
            }
        }

        static string ReadPassword()
        {
            string password = "";
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;

                if (key != ConsoleKey.Backspace && key != ConsoleKey.Enter)
                {
                    password += keyInfo.KeyChar;
                    Console.Write("*");
                }
                else if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            } while (key != ConsoleKey.Enter);
            Console.WriteLine();
            return password;
        }
    }
}
