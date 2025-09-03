namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersMabPlayerTurnRequest
    {        
        public required string UserdId { get; set; }
        public int? Mab_PlayerCardId { get; set; }
    }
}
