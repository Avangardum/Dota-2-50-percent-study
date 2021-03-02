using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Dota_2_50_percent_study
{
    public static class WebAPI
    {
        public static string Key = "NoKey";

        private static Match[] GetMatchesByRequest(string strRequest, int amount = 100)
        {
            WebRequest request = WebRequest.Create(strRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseString = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
            
            JObject root = JObject.Parse(responseString);
            JObject[] jMatches = JArray.FromObject(root["result"]["matches"]).Values<JObject>().ToArray();
            
            
            Match[] matches = new Match[amount];
            for (int i = 0; i < 100; i++)
            {
                JObject jMatch = jMatches[i];
                Match match = new Match();
                matches[i] = match;
                match.Id = jMatch["match_id"].ToObject<ulong>();
                match.MatchSequenceNumber = jMatch["match_seq_num"].ToObject<ulong>();
                match.Winner = jMatch["radiant_win"].ToObject<bool>() ? Team.Radiant : Team.Dire;
                JObject[] jPlayers = JArray.FromObject(jMatch["players"]).Values<JObject>().ToArray();
                match.RadiantPlayers = new Player[5];
                match.DirePlayers = new Player[5];
                for (int j = 0; j < jPlayers.Length; j++)
                {
                    JObject jPlayer = jPlayers[j];
                    Player player = new Player();
                    player.Match = match;
                    player.HasId = jPlayer["account_id"] != null;
                    if (player.HasId)
                    {
                        player.Id = jPlayer["account_id"].ToObject<uint>();
                    }
                    int playerSlot = jPlayer["player_slot"].ToObject<int>();
                    int teamMask = 128; // 1000 0000
                    int positionMask = 7; // 0000 0111
                    player.Team = (Team)((playerSlot & teamMask) >> 7); // 0 - radiant, 1 - dire
                    player.Position = playerSlot & positionMask;
                    if (player.Team == Team.Radiant)
                    {
                        match.RadiantPlayers[player.Position] = player;
                    }
                    else
                    {
                        match.DirePlayers[player.Position] = player;
                    }
                }
            }

            return matches;
        }
        
        public static Match[] GetLast100Matches()
        {
            return GetMatchesByRequest($"https://api.steampowered.com/IDOTA2Match_570/GetMatchHistory/V001/?key={Key}&min_players=10&matches_requested=100");
        }

        public static Match[] Get100MatchesStartingFromId(ulong id)
        {
            return GetMatchesByRequest($"https://api.steampowered.com/IDOTA2Match_570/GetMatchHistory/V001/?key={Key}&min_players=10&matches_requested=100&start_at_match_id={id}");
        }

        public static Match[] Get100MatchesStartingFromSequenceNumber(ulong sequenceNumber)
        {
            return GetMatchesByRequest(
                $"https://api.steampowered.com/IDOTA2Match_570/GetMatchHistoryBySequenceNum/V001/?key={Key}&min_players=10&matches_requested=100&start_at_match_seq_num={sequenceNumber}");
        }

        public static Match[] GetMatches(int amount, ulong startFromSequenceNumber)
        {
            Console.WriteLine($"Getting {amount} matches");

            Match[] matches = new Match[amount];
            int nextElementIndex = 0;
        
            Match[] _100Matches = Get100MatchesStartingFromSequenceNumber(startFromSequenceNumber);
        
            while (true)
            {
                foreach (Match match in _100Matches)
                {
                    if (Array.Exists(matches, x => x != null && x.Id == match.Id)) // if match is already exists in matches array, dont add it again 
                    {
                        continue;
                    }
                    if (match.RadiantPlayers.Length != 5 || match.DirePlayers.Length != 5 || 
                        match.RadiantPlayers.Contains(null) || match.DirePlayers.Contains(null)) // if there are not 10 players in the match, don't add it
                    {
                        continue;
                    }
                
                    matches[nextElementIndex] = match;
                    nextElementIndex++;
                    if (nextElementIndex >= amount)
                    {
                        Console.WriteLine($"{nextElementIndex}/{amount}");
                        return matches;
                    }
                }
                Console.WriteLine($"{nextElementIndex}/{amount}");
            
                Match newestMatch = _100Matches[0];
                foreach (Match match in _100Matches)
                {
                    if (match.MatchSequenceNumber > newestMatch.MatchSequenceNumber)
                    {
                        newestMatch = match;
                    }
                }

                bool success = false;
                while (!success)
                {
                    try
                    {
                        _100Matches = Get100MatchesStartingFromSequenceNumber(newestMatch.MatchSequenceNumber);
                        success = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error while getting data from the web API. Retrying in 30 seconds");
                        success = false;
                        Thread.Sleep(30000);
                    }
                }
            }
        }
    }
}