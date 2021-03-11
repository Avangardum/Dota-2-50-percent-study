using System;
using System.IO;
using System.Text;
using MySql.Data.MySqlClient;

namespace Dota_2_50_percent_study
{
    public static class Database
    {
        private const string EmptyDatabaseDumpPath = "empty_db_dump.sql";
        
        public static string Username;
        public static string Password;

        private static MySqlConnection _connection;

        public static void Connect()
        {
            string connStr = $"server=localhost;user={Username};database=dota_2_50_percent_study;password={Password};";
            _connection = new MySqlConnection(connStr);
            _connection.Open();
        }

        public static void AddMatches(params Match[] matches)
        {
            MySqlCommand command;
            int matchesInCommand = 0;
            StringBuilder commandStr = new StringBuilder();
            int rowsAffected;
            
            Console.WriteLine("Writing data to the database");
            
            
            foreach (Match match in matches)
            {
                if (match != null)
                {
                    commandStr.Append(
                        $"INSERT IGNORE INTO matches (id, match_sequence_number, winner) VALUES ({match.Id}, {match.MatchSequenceNumber}, {(int) match.Winner});\n");
                    
                    foreach (var player in match.DirePlayers)
                    {
                        commandStr.Append(
                            $"INSERT IGNORE INTO players (id, match_id, team, is_winner) VALUES ({player.Id}, {player.Match.Id}, {(int) Team.Dire}, {player.IsWinner});\n");
                    }

                    foreach (var player in match.RadiantPlayers)
                    {
                        commandStr.Append(
                            $"INSERT IGNORE INTO players (id, match_id, team, is_winner) VALUES ({player.Id}, {player.Match.Id}, {(int) Team.Radiant}, {player.IsWinner});\n");
                    }

                    matchesInCommand++;
                    if (matchesInCommand >= 100 && commandStr.Length > 0)
                    {
                        command = new MySqlCommand(commandStr.ToString(), _connection);
                        rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine($"SQL command executed, {rowsAffected} rows affected");
                        commandStr = new StringBuilder();
                        matchesInCommand = 0;
                    }
                }
            }
            if (commandStr.Length > 0)
            {
                command = new MySqlCommand(commandStr.ToString(), _connection);
                rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"SQL command executed, {rowsAffected} rows affected");
            }
        }

        public static ulong GetLastMatchId()
        {
            string commandStr = "SELECT MAX(match_sequence_number) FROM matches;";
            MySqlCommand command = new MySqlCommand(commandStr, _connection);
            return (ulong) command.ExecuteScalar();
        }

        public static bool HasMatches()
        {
            string commandStr = "SELECT COUNT(*) FROM matches";
            MySqlCommand command = new MySqlCommand(commandStr, _connection);
            return (long) command.ExecuteScalar() > 0;
        }

        public static void CreateDatabase()
        {
            MySqlScript script = new MySqlScript(_connection, File.ReadAllText(EmptyDatabaseDumpPath));
            script.Execute();
            Console.WriteLine("Database created");
        }

        public static Player[] GetPlayersWhereStreakIsNull(int amount)
        {
            string commandStr = "SELECT id, match_id FROM players WHERE streak IS NULL;";
            MySqlCommand command = new MySqlCommand(commandStr, _connection);
            var reader = command.ExecuteReader();
            
            Player[] result = new Player[amount];
            int i = 0;
            
            if (reader.HasRows)
            {
                while (reader.Read() && i < amount)
                {
                    result[i] = new Player {Id = reader.GetUInt64(0), Match = new Match {Id = reader.GetUInt64(1)}};
                    i++;
                }
            }

            reader.Close();
            return result;
        }

        public static void SetPlayerStreak(Player player, int streak)
        {
            string commandStr = $"UPDATE players SET streak = {streak} WHERE id = {player.Id};";
            MySqlCommand command = new MySqlCommand(commandStr, _connection);
            command.ExecuteScalar();
        }
    }
}