using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using RedisLayer;

namespace BetMeScraping
{
    public class Scrapeing
    {
        private int numberOfParticipants { get; set; }
        private int database { get; set; }
        private string gameId { get; set; }
        private const string eventSourceName = "BetScraping";


        public Scrapeing(string betmeType)
        {
            if (betmeType.Equals("ezy", StringComparison.OrdinalIgnoreCase))
            {
                numberOfParticipants = 11;
                database = 1;
                gameId = "kvRK";
            }
            else
            {
                numberOfParticipants = 21;
                database = 2;
                gameId = "KPfu";

            }
        }

        private void WriteEvent(string msg)
        {
            try
            {
                EventLog.WriteEntry(eventSourceName, msg, EventLogEntryType.Information);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        public void ScrapeBetme()
        {

            IWebDriver driver = new ChromeDriver("c:/seleniumDrivers");
            var redis = new Redis(database);



            try
            {
                if (!EventLog.SourceExists(eventSourceName))
                    EventLog.CreateEventSource(eventSourceName, "Application");
                for (var i = 0; i < numberOfParticipants; i++)
                {

                    driver.Navigate().GoToUrl($"http://www.betme.se/game.html?id={gameId}");
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

                    var usersTabSelect = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("usersTabSelect")));
                    var userForm = TryAgainException(() =>
                    {
                        usersTabSelect.Click();
                        return wait.Until(ExpectedConditions.ElementIsVisible(By.Id("usersForm")));
                    });

                    var participantsRow = userForm.FindElements(By.CssSelector("tr"));
                    var partRow = participantsRow[i];

                    var participant = partRow.FindElement(By.CssSelector("a"));
                    var participantScore = partRow.FindElements(By.CssSelector("td"))[2];
                    var points = participantScore.Text.Replace("Points", "");
                    var participantText = $"{participant.Text} {points}";

                    Console.WriteLine(participantText);
                    WriteEvent(participantText);

                    var user = new Participant
                    {

                        Name = participant.Text,
                        Points = points,
                        Matches = new List<Match>()
                    };


                    if (participant.Displayed)
                    {
                        participant.Click();
                    }
                    else
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", participant);
                    }
                    ReadOnlyCollection<IWebElement> boxes = null;
                    boxes = TryAgainException(() =>
                    {
                        IWebElement userTabToRender = null;
                        userTabToRender = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("userTabToRender")));
                        return userTabToRender.FindElements(By.ClassName("boxHalfInTab"));
                    });

                    var sb = new StringBuilder();
                    foreach (var box in boxes)
                    {
                        IWebElement table = null;
                        table = TryAgainException(() => box.FindElement(By.ClassName("eventRow")));

                        ReadOnlyCollection<IWebElement> rows = null;
                        rows = TryAgainException(() => table.FindElements(By.CssSelector("tr:not(.rowSeparator)")));

                        int rowIndex = 0;
                        ReadOnlyCollection<IWebElement> rowSeparators = null;
                        rowSeparators = TryAgainException(() => table.FindElements(By.CssSelector("tr.rowSeparator")));
                        foreach (var row in rows)
                        {
                            var match = new Match();
                            var homeTeam = TryAgainException(() => row.FindElement(By.ClassName("eventRowLeft")));
                            sb.Append(homeTeam.Text);
                            match.HomeTeam = homeTeam.Text;

                            sb.Append(" ");

                            var events = TryAgainException(() => row.FindElement(By.ClassName("eventRowCenter")));
                            var fields = TryAgainException(() => events.FindElements(By.CssSelector("input")));

                            var result = fields[0].GetAttribute("value") + " - " + fields[1].GetAttribute("value");

                            sb.Append(result);
                            sb.Append(" ");
                            match.Result = result;

                            var awayTeam = TryAgainException(() => row.FindElement(By.ClassName("eventRowRight")));
                            sb.AppendLine(awayTeam.Text + " ");
                            match.AwayTeam = awayTeam.Text;

                            var matchTime = rowSeparators[rowIndex++].FindElement(By.CssSelector("span")).Text;
                            sb.AppendLine(matchTime);

                            match.MatchStart = DateTime.Parse(matchTime);
                            user.Matches.Add(match);
                        }
                        sb.AppendLine("-------------------------------------------------");
                    }
                    Console.WriteLine(sb.ToString());
                    user.LastUpdated = DateTime.Now.ToString("O");
                    sb.Insert(0, user.LastUpdated + Environment.NewLine);
                    WriteEvent(sb.ToString());

                    redis.SetRedisValue(user, "participant_" + (i + 1));
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                driver.Quit();
            }
        }


        public static T TryAgainException<T>(Func<T> action)
        {
            return TryAgain(action, 0, 3);
        }

        private static T TryAgain<T>(Func<T> action, int i, int limit)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                if (i >= limit)
                {
                    throw e;
                }
                var j = i + 1;
                return TryAgain(action, j, limit);
            }


        }
    }
}