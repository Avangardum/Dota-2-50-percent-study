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
                input = Console.ReadLine();
                switch (input)
                {
                    case "2" :
                        AddMoreMatchesToTheDatabase();
                        break;
                }
            }
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
    }
}