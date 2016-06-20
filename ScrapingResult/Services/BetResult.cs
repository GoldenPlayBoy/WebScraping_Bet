using System;
using System.Collections.Generic;
using System.Linq;
using RedisLayer;
using ScrapingResult.Models;

namespace ScrapingResult.Services
{
    public class BetResult
    {
        public static List<MatchesResult> GetBetResult(int database)
        {
            var redis = new Redis(database);
            var keys = redis.GetAllKeys();
            var participants = new List<Participant>();
            keys.Sort();
            var ordered = keys.OrderBy(x => x.Length);
            foreach (var key in ordered)
            {
                var p = redis.GetRedisValue<Participant>(key);
                participants.Add(p);
            }
            var firstPart = participants.First();
            var orderdMatches = firstPart.Matches
                .Where(x => x.MatchStart > DateTime.Now.AddHours(-5))
                .OrderBy(x => x.MatchStart < DateTime.Now).ToList();

            var matches = new List<MatchesResult>();
            foreach (var match in orderdMatches)
            {
                var matchesResult = new MatchesResult
                {
                    MatchStart = match.MatchStart,
                    HomeTeam = match.HomeTeam,
                    AwayTeam = match.AwayTeam,
                    BettingResults = new List<BettingResult>()
                };
                participants.ForEach(x =>
                {
                    var bet = new BettingResult
                    {
                        Name = x.Name,
                        Points = x.Points
                    };

                    var partMatch = x.Matches.First(u => u.HomeTeam.Equals(match.HomeTeam) 
                                                        && u.AwayTeam.Equals(match.AwayTeam)
                                                        && u.MatchStart == match.MatchStart);
                    bet.Bet = partMatch.Result;

                    matchesResult.BettingResults.Add(bet);
                });
                matches.Add(matchesResult);
            }

            return matches.ToList();
        }
    }
}