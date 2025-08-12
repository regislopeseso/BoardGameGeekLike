namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersShowActiveMabDeckDetailsResponse
    {
        public int? ActiveMabDeckId { get; set; }
        public string? ActiveMabDeckName { get; set; }
        public List<UsersShowActiveMabDeckDetailsResponse_mabCard>? ActiveMabPlayerCards { get; set; }
    }
}
