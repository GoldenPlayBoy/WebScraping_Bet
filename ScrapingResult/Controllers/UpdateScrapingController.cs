using System.Diagnostics;
using System.IO;
using System.Web.Http;

namespace ScrapingResult.Controllers
{
    public class UpdateScrapingController : ApiController
    {
        public IHttpActionResult Get(string action)
        {
            var fileName = "betmeScraping.exe";
            var pathAndFile = string.Empty;
            if (File.Exists($@"C:\ScrapingLib\{fileName}"))
            {
                pathAndFile = $@"C:\ScrapingLib\{fileName}";
            }
            else if (File.Exists($@"D:\Projekt\Scraping\BetmeScraping\BetmeScraping\bin\Debug\{fileName}"))
            {
                pathAndFile = $@"D:\Projekt\Scraping\BetmeScraping\BetmeScraping\bin\Debug\{fileName}";
            }

            Process.Start(pathAndFile, action);

            var eventSourceName = "BetScraping";
            if (!EventLog.SourceExists(eventSourceName))
                EventLog.CreateEventSource(eventSourceName, eventSourceName);

            EventLog.WriteEntry(eventSourceName, $"Process start: {pathAndFile} action: {action}", EventLogEntryType.Information);

            return Ok($"process start:{pathAndFile}");
        }
    }
}
