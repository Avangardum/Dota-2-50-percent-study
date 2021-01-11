namespace Dota_2_50_percent_study
{
    /// <summary>
    /// Stores a condition of the player in a specific match
    /// </summary>
    public class Player
    {
        public ulong Id;
        public Match Match;
        public Team Team;
        public int Position;
        public bool HasId;
    }
}