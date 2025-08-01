namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsGetNpcsResponse
    {
        public int? NpcId { get; set; }

        public string? NpcName { get; set; }

        public string? NpcDescription { get; set; }

        public int? NpcLevel { get; set; }

        public bool? NpcIsDeleted { get; set; }

        public List<AdminsGetNpcsResponse_Deck>? Deck { get; set; }
    }
}
