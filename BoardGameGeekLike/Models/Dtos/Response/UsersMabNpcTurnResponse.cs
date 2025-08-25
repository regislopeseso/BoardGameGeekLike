namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabNpcTurnResponse
    {
        public bool? HasPlayerWon { get; set; }

        public int? MabBattleRoundPoints { get; set; } = 0;

        public bool? IsRoundFinished { get; set; } = false;
    }
}
