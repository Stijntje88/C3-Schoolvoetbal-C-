namespace GokApp_PRA.Model
{
    public class Bet
    {
        public int Id { get; set; }
        public string ChosenMatch { get; set; }
        public string ChosenTeam { get; set; }
        public int Amount { get; set; }  // Amount bet on the match
        public bool IsWon { get; set; }  // Track if the bet was won
        public bool IsBetPlaced { get; set; }  // Track if the bet was placed
    }
}
