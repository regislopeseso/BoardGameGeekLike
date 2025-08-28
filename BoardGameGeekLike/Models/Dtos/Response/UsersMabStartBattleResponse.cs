namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabStartBattleResponse
    {
        public string? MabPlayerNickName { get; set; }

        public string? MabNpcName { get; set; }

        public bool? DoesPlayerGoFirst { get; set; }

        public int? MabBattleRoundNumber { get; set; } = 0;        
    }
}
