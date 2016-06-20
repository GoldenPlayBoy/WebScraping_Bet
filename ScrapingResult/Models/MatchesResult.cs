using System;
using System.Collections.Generic;

namespace ScrapingResult.Models
{
    public class MatchesResult  
    {
        public DateTime MatchStart { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public List<BettingResult> BettingResults { get; set; }
    }
    public class BettingResult
    {
        public string Name { get; set; }
        public string Bet { get; set; }
        public string Points { get; set; }
    }
}