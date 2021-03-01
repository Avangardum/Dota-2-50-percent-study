using System;
using System.Text;
using MySql.Data.MySqlClient;

namespace Dota_2_50_percent_study
{
    public static class Database
    {
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
            StringBuilder commandStr = new StringBuilder();
            foreach (Match match in matches)
            {
                commandStr.Append(
                    $"INSERT IGNORE INTO matches (id, match_sequence_number, winner) VALUES ({match.Id}, {match.MatchSequenceNumber}, {(int) match.Winner});\n");
                foreach (var player in match.DirePlayers)
                {
                    commandStr.Append(
                        $"INSERT INTO players (id, match_id, team) VALUES ({player.Id}, {player.Match.Id}, {(int) Team.Dire});");
                }

                foreach (var player in match.RadiantPlayers)
                {
                    commandStr.Append(
                        $"INSERT INTO players (id, match_id, team) VALUES ({player.Id}, {player.Match.Id}, {(int) Team.Radiant});");
                }
            }
            MySqlCommand command = new MySqlCommand(commandStr.ToString(), _connection);
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine($"SQL command executed, {rowsAffected} rows affected");
        }
    }
}