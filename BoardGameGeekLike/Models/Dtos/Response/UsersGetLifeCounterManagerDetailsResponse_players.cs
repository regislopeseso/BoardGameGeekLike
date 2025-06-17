namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLifeCounterManagerDetailsResponse_players
    {     
        public int? PlayerId { get; set; }

        public string? PlayerName { get; set; }
       
        public int? CurrentLifePoints { get; set; }
       
        public bool? IsDefeated { get; set; }      
    }
}
