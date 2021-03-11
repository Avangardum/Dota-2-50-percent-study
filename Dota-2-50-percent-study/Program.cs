using System;
using System.IO;
using System.Text;
using ConsoleTables;

namespace Dota_2_50_percent_study
{
    internal class Program
    {
        private const string KeyPath = "key.txt";
        private const string DBUserNamePath = "dbusername.txt";
        private const string DBPasswordPath = "dbpassword.txt";
        
        public static void Main(string[] args)
        {
            if (!File.Exists(KeyPath))
            {
                Console.WriteLine("Enter your Steam Web API Key. You can get it here: https://steamcommunity.com/dev/apikey");
                string key = Console.ReadLine();
                File.WriteAllText(KeyPath, key);
                Console.WriteLine();
            }
            WebAPI.Key = File.ReadAllText(KeyPath);
            
            if (!File.Exists(DBUserNamePath))
            {
                Console.WriteLine("Enter your MySQl database username");
                string username = Console.ReadLine();
                File.WriteAllText(DBUserNamePath, username);
                Console.WriteLine();
            }
            Database.Username = File.ReadAllText(DBUserNamePath);
            
            if (!File.Exists(DBPasswordPath))
            {
                Console.WriteLine("Enter your MySQl database password");
                string password = Console.ReadLine();
                File.WriteAllText(DBPasswordPath, password);
                Console.WriteLine();
            }
            Database.Password = File.ReadAllText(DBPasswordPath);
            
            Database.Connect();

            string input = String.Empty;
            while (input != "q")
            {
                Console.WriteLine(@"
Choose subroutine:
1 - create database (will delete existing data)
2 - add more matches (and players) to the database
3 - calculate players' win/lose streaks
4 - analyze streak data
q - quit
");
                input = Console.ReadLine().ToLower();
                switch (input)
                {
                    case "1" :
                        CreateDatabase();
                        break;
                    case "2" :
                        AddMoreMatchesToTheDatabase();
                        break;
                    case "3" :
                        CalculatePlayerStreaks();
                        break;
                    case "4":
                        AnalyzeStreakData();
                        break;
                }
            }
        }

        public static void CreateDatabase()
        {
            Console.WriteLine("WARNING! This action will delete existing database. Proceed? (y/n)");
            string input = string.Empty;
            while (input != "y")
            {
                input = Console.ReadLine().ToLower();
                if (input == "n")
                {
                    return;
                }
            }
            
            Database.CreateDatabase();
        }
        
        public static void AddMoreMatchesToTheDatabase()
        {
            {
                Console.WriteLine("How many matches to add?");
                int amount = int.Parse(Console.ReadLine());

                ulong startFromSequenceNumber;
                if (Database.HasMatches())
                {
                    startFromSequenceNumber = Database.GetLastMatchId();
                }
                else
                {
                    Console.WriteLine("Enter the sequence number (not id) of the match you want to start from");
                    startFromSequenceNumber = ulong.Parse(Console.ReadLine());
                }
                
                var matches = WebAPI.GetMatches(amount, startFromSequenceNumber);

                Database.AddMatches(matches);

                #region DrawTable

                // ConsoleTable table = new ConsoleTable("Match", "Radiant players", "Dire players");
                // foreach (var match in matches)
                // {
                //     StringBuilder radiantPlayers = new StringBuilder();
                //     foreach (Player player in match.RadiantPlayers)
                //     {
                //         radiantPlayers.Append(player.Id);
                //         radiantPlayers.Append(" ");
                //     }
                //     StringBuilder direPlayers = new StringBuilder();
                //     foreach (Player player in match.DirePlayers)
                //     {
                //         direPlayers.Append(player.Id);
                //         direPlayers.Append(" ");
                //     }
                //     table.AddRow(match.Id, radiantPlayers, direPlayers);
                // }
                // table.Write();

                #endregion
            }
        }

        public static void CalculatePlayerStreaks()
        {
            Console.WriteLine("How many players to process?");
            int amount = int.Parse(Console.ReadLine());

            var players = Database.GetPlayersWhereStreakIsNull(amount);

            for (int i = 0; i < amount; i++)
            {
                if (players[i] == null)
                {
                    Console.WriteLine("All players processed");
                    break;
                }

                int streak = WebAPI.GetPlayerStreak(players[i]);
                
                Database.SetPlayerStreak(players[i], streak);

                if ((i + 1) % 10 == 0)
                {
                    Console.WriteLine($"{i + 1}/{amount}");
                }
            }
            
            Console.WriteLine("Done");
        }

        public static void AnalyzeStreakData()
        {
            var data = Database.GetWinrateByStreakData(-10, 10);
            ConsoleTable table = new ConsoleTable("Streak", "Winrate", "Players");
            foreach (var item in data)
            {
                table.AddRow(item.Item1, (item.Item2 * 100).ToString("00.00") + "%", item.Item3);
            }
            Console.WriteLine(table);
        }
    }
}