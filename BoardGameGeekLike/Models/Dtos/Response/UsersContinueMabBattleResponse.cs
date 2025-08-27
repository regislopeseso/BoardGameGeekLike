namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersContinueMabBattleResponse
    {
        public string? MabPlayerNickName { get; set; }

        public string? MabNpcName { get; set; }
  
        public bool? IsPlayerTurn { get; set; }

        public int? MabBattleRoundNumber { get; set; }

        public int? MabBattlePoints { get; set; }

        public UsersContinueMabBattleResponse_MabNpcCard? MabNpcCard { get; set; }
    }
}
