using System;
using System.IO;
using System.Text;
using ConsoleTables;

namespace Dota_2_50_percent_study
{
    internal class Program
    {
        private const string KeyPath = "key.txt";
        
        public static void Main(string[] args)
        {
            if (!File.Exists(KeyPath))
            {
                Console.Write("Enter your Steam Web API Key. You can get it here: https://steamcommunity.com/dev/apikey\n>");
                string key = Console.ReadLine();
                File.WriteAllText(KeyPath, key);
            }
            WebAPI.Key = File.ReadAllText(KeyPath);

            var matches = WebAPI.GetMatches(500, 4866994034);
            
            ConsoleTable table = new ConsoleTable("Match", "Radiant players", "Dire players");
            foreach (var match in matches)
            {
                StringBuilder radiantPlayers = new StringBuilder();
                foreach (Player player in match.RadiantPlayers)
                {
                    radiantPlayers.Append(player.Id);
                    radiantPlayers.Append(" ");
                }
                StringBuilder direPlayers = new StringBuilder();
                foreach (Player player in match.DirePlayers)
                {
                    direPlayers.Append(player.Id);
                    direPlayers.Append(" ");
                }
                table.AddRow(match.Id, radiantPlayers, direPlayers);
            }
            table.Write();

            Console.Write("Press any button to quit...");
            Console.ReadKey();
        }
    }
}