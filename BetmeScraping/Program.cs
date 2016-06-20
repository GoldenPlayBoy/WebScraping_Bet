namespace BetMeScraping
{
    class Program
    {
        static void Main(string[] args)
        {

            var betmeType = args.Length > 0 ? args[0] : string.Empty;
            var scrapeing = new Scrapeing(betmeType);
            scrapeing.ScrapeBetme();
            
        }
    }
}
