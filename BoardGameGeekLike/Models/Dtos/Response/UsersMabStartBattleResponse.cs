namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabStartBattleResponse
    {
        public string? Mab_PlayerNickName { get; set; }

        public string? MabNpcName { get; set; }

        public bool? Mab_DoesPlayerGoFirst { get; set; }

        public int? Mab_DuelNumber { get; set; } = 1;        
    }
}
