namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabNpcTurnResponse
    {
        public int? WinnerCardCopyId { get; set; }

        public int? MabBattleRoundPoints { get; set; } = 0;

        public bool? IsRoundFinished { get; set; } = false;
    }
}
