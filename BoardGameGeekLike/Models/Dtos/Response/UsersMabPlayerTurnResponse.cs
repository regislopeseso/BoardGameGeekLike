namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabPlayerTurnResponse
    {
        public int? WinnerCardCopyId { get; set; }

        public int? MabBattleRoundPoints { get; set; } = 0;

        public bool? IsRoundFinished { get; set; } = false;
    }
}
