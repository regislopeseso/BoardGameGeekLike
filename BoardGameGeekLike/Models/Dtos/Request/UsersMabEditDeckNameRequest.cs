namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersMabEditDeckNameRequest
    {      
        public int? Mab_DeckId { get; set; }
        public string? Mab_DeckNewName { get; set; }      
    }
}
