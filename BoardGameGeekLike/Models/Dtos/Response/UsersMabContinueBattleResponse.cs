namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabContinueBattleResponse
    {
        public string? MabPlayerNickName { get; set; }

        public string? MabNpcName { get; set; }
  
        public bool? IsPlayerTurn { get; set; }

        public int? MabBattleRoundNumber { get; set; }

        public int? MabBattlePoints { get; set; }

        public UsersMabContinueBattleResponse_NpcCard? MabNpcCard { get; set; }
    }
}
