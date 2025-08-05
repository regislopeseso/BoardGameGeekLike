namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsEditMabNpcRequest
    {
        public int? NpcId { get; set; }

        public string? NpcName { get; set; }

        public string? NpcDescription { get; set; }

        public List<int>? CardIds { get; set; }
    }
}
