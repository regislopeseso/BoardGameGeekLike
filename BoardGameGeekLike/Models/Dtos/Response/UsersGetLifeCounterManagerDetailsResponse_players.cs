namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLifeCounterManagerDetailsResponse_players
    {     
        public string? PlayerName { get; set; }
        public int? CurrentLife { get; set; }        
        public bool? IsDefeated { get; set; }      
    }
}
