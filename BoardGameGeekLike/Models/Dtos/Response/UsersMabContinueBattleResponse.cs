namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabContinueBattleResponse
    {
        public string? Mab_PlayerNickName { get; set; }

        public string? Mab_NpcName { get; set; }
  
        public bool? Mab_IsPlayerTurn { get; set; }

        public int? Mab_DuelNumber { get; set; }

        public int? Mab_CurrentBattlePoints { get; set; }

        public UsersMabContinueBattleResponse_NpcCard? Mab_NpcCard { get; set; }
    }
}
