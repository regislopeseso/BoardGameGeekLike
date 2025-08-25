namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabPlayerTurnResponse
    {
        public bool? HasPlayerWon { get; set; }

        public int? MabBattleRoundPoints { get; set; } = 0;

        public bool? IsRoundFinished { get; set; } = false;
    }
}
