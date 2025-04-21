using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Main.Launches
{
    internal class TabbedLauncher : Ilauncher
    {
        //Chrome Window
        ChromeDriver driverMain;
        readonly int followersCheckRepeats = 5;

        public TabbedLauncher()
        {
            //Creates Chrome Window
            driverMain = Program.LaunchChromeWithDebugging();
        }

        public void Start()
        {
            Thread.Sleep(1000);

            CheckIfLoggedIn();

            GetFollowersRepeated();

            UnfollowUnneeded();

            Program.CountDownQuitTime(driverMain, 60);
        }

        void GetFollowersRepeated()
        {
            int lastFollowersCount;

            int retries = 0;
            int maxRetries = 5;

            do
            {
                lastFollowersCount = Program.accountData.Followers.Count;
                driverMain.Navigate().GoToUrl($"https://www.instagram.com/{Program.accountData.Name}/");

                //Finds the "Followers" button to show the list of all Followers
                WaitForElement(By.XPath($"//a[contains(@href, '/{Program.accountData.Name}/followers/')]")).Click();

                GetAllFollowers();

                if (lastFollowersCount == Program.accountData.Followers.Count)
                {
                    retries++;
                }
                else retries = 0;

            } while(retries < maxRetries);

        }

        static int GetFollowersCount(IWebElement scrollListContainer)
        {
            int scrollListCount = scrollListContainer.FindElements(By.XPath("./*")).Count;
            return scrollListCount;
        }

        void ScrollToBottom(string scrollableElementXpath, string scrollListContainerXpath)
        {
            int previousCount = 0;
            int sameCount = 0;
            int retries = 0;
            int maxRetries = 5;

            do
            {
                previousCount = sameCount;

                var scrollableElement = WaitForElement(By.XPath(scrollableElementXpath));
                var scrollListXpath = WaitForElement(By.XPath(scrollListContainerXpath));

                ScrollDown(scrollableElement);
                sameCount = GetFollowersCount(scrollListXpath);

                if (previousCount == sameCount)
                {
                    retries++;
                }
                else retries = 0;

                Thread.Sleep(2000);


            } while (retries < maxRetries);
        }

        void ScrollDown(IWebElement scrollableElement)
        {
            ((IJavaScriptExecutor)driverMain).ExecuteScript(
                "arguments[0].scrollTop = arguments[0].scrollHeight", scrollableElement);
        }

        void GetAllFollowers()
        {
            string scrollableElementXPath = "//div[contains(@class, 'xyi19xy')]";

            string scrollListXpath = "//div[contains(@class, 'xyi19xy')]/div[1]/div[1]";

            WaitUntilElementHasChildren(By.XPath(scrollListXpath), By.XPath("./*"));

            Thread.Sleep(1000);

            ScrollToBottom(scrollableElementXPath, scrollListXpath);

            SetFollowersList(scrollListXpath);

            Program.accountData.Save();

            Console.WriteLine($"Saved {Program.accountData.Followers.Count} Followers");
        }

        void SetFollowersList(string scrollListContainerXpath)
        {

            var scrollList = WaitForElement(By.XPath(scrollListContainerXpath));

            HashSet<string> followersNames = [.. scrollList.FindElements(By.XPath(".//*[contains(@class, '_ap3a')]")).Select(e => e.Text).Where(e => e != "Follow")];

            //If there is not saved amount then save all
            if (Program.accountData.Followers == null || Program.accountData.Followers.Count == 0)
            {
                Program.accountData.Followers = followersNames;
                return;
            }

            Program.accountData.Followers.UnionWith(followersNames);

            
        }

        void CheckIfLoggedIn()
        {
            GoToHomePage();

            //Checks if there is a user saved, if not then find and save
            if (Program.accountData.Name == null || Program.accountData.Name.Length == 0)
            {
                driverMain.Navigate().GoToUrl("https://www.instagram.com/");
                Program.accountData.Name = WaitForElement(By.XPath("/html/body/div[1]/div/div/div[2]/div/div/div[1]/div[1]/div[2]/div/div/div/div/div[2]/div[8]/div/span/div/a")).GetAttribute("href")
                    .Replace("https://www.instagram.com", "")
                    .Replace("/", "");
                Program.accountData.Save();
            }

        }


        void GoToHomePage()
        {
            driverMain.Navigate().GoToUrl("https://www.instagram.com/accounts/login");

            Thread.Sleep(2000);

            //Checks if a login is required
            if (driverMain.Url == "https://www.instagram.com/accounts/login/#") Login();
        }

        public void Login()
        {

            Console.WriteLine($"\r\nAttemping Login with: \r\n Username: {Program.accountData.Username} \r\n Password: {Program.accountData.Password} \n");

            Program.SendKey(driverMain, Program.accountData.Username, "Username", "email");
            Program.SendKey(driverMain, Program.accountData.Password, "Password", "pass");

            Thread.Sleep(1000);

            driverMain.FindElement(By.XPath("//button[@type='submit']")).Click();

            Thread.Sleep(5000);

        }

        public IWebElement WaitForElement(By locator, int timeoutSeconds = 10)
        {
            var localWait = new WebDriverWait(driverMain, TimeSpan.FromSeconds(timeoutSeconds));
            return localWait.Until(d => d.FindElement(locator));
        }

        public IWebElement WaitForElement(IWebElement parent, By locator, int timeoutSeconds = 10)
        {
            var localWait = new WebDriverWait(driverMain, TimeSpan.FromSeconds(timeoutSeconds));
            return localWait.Until(d => parent.FindElement(locator));
        }

        public void WaitUntilElementHasChildren(By parentLocator, By childLocator, int timeoutSeconds = 10)
        {
            var wait = new WebDriverWait(driverMain, TimeSpan.FromSeconds(timeoutSeconds));

            wait.Until(d =>
            {
                var parent = d.FindElement(parentLocator);
                var children = parent.FindElements(childLocator);
                return children.Count > 0;
            });
        }

        public void UnfollowUnneeded()
        {
            driverMain.Navigate().GoToUrl($"https://www.instagram.com/{Program.accountData.Name}/");
            //Finds the "Followers" button to show the list of all Followers
            WaitForElement(By.XPath("//a[contains(@href, '/following/') and .//span[contains(text(), 'following')]]")).Click();

            string scrollableElementXPath = "//div[contains(@class, 'xyi19xy')]";

            string scrollListXpath = "//div[contains(@class, 'xyi19xy')]/div[1]/div[1]";

            ManageFollowing(scrollableElementXPath, scrollListXpath);

        }

        void ManageFollowing(string scrollableElementXPath, string scrollListXpath)
        {

            var scrollList = WaitForElement(By.XPath(scrollListXpath));

            WaitUntilElementHasChildren(By.XPath(scrollListXpath), By.XPath("./*"));

            Thread.Sleep(1000);

            HashSet<IWebElement> followingElements = [.. WaitForElement(By.XPath(scrollListXpath)).FindElements(By.XPath("./*"))];

            while (!Program.programData.ReachedDailyLimit())
            {
                UnfollowFromCollection(followingElements);

                IWebElement scrollableElement = driverMain.FindElement(By.XPath(scrollableElementXPath));
                IWebElement lastAccount = followingElements.Last();
                ScrollDown(scrollableElement);

                Thread.Sleep(2000);

                followingElements = [.. lastAccount.FindElements(By.XPath("following-sibling::*"))];

                if (followingElements.Count == 0 || lastAccount != followingElements.Last())
                {
                    Console.WriteLine("\r\n---No More Followers Left To Check!");
                    return;
                }
            }

        }

        
        void UnfollowFromCollection(HashSet<IWebElement> followingElements)
        {
            string acountPath = "./div/div/div";
            string accountNamePath = "./div[2]";
            string accountFollowingButtonPath = "./div[3]";

            foreach (var following in followingElements)
            {
                IWebElement acount = following.FindElement(By.XPath(acountPath));

                string accountName = acount.FindElement(By.XPath(accountNamePath)).FindElement(By.XPath(".//*[contains(@class, '_ap3a')]")).Text;

                if (Program.accountData.Followers.Contains(accountName))
                {
                    Console.WriteLine($"\r\n{accountName} - Follows");
                    continue;
                }

                Console.WriteLine($"\r\n{accountName} - Does Not Follow You Back (Unfollowing)");

                acount.FindElement(By.XPath($"{accountFollowingButtonPath}//button")).Click();
                driverMain.FindElement(By.XPath("//button[text()='Unfollow']")).Click();

                Program.programData.Unfollowed++;
                Program.programData.Save();

                if(Program.programData.ReachedDailyLimit()) return;

                Program.ResumeCountDown(new Random().Next(60, 80));
            }

        }

        static IWebElement GetLastElement(IWebElement scrollList)
        {
            return scrollList.FindElement(By.XPath("./*[last()]"));
        }

        static void SleepRandomAmount(int min, int max)
        {
            var rand = new Random().Next(min, max);
            Thread.Sleep(rand);
        }
    }
}
