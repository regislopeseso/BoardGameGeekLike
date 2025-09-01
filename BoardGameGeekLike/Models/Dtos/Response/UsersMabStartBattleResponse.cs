namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabStartBattleResponse
    {
        public string? Mab_PlayerNickName { get; set; }

        public string? Mab_NpcName { get; set; }

        public bool? Mab_IsPlayerTurn { get; set; }

        public int? Mab_DuelNumber { get; set; } = 1;        
    }
}
