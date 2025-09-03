namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersMabGetRandomNpcIdRequest
    {
        public int? Mab_PlayerLevel { get; set; }

        public List<int>? Mab_DefeatedNpcsIds { get; set; }
    }
}
