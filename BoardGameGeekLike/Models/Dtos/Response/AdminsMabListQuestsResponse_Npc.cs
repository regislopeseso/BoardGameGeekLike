namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsMabListQuestsResponse_Npc
    {
        public int? Mab_NpcId { get; set; }

        public string? Mab_NpcName { get; set; }

        public int? Mab_NpcLevel { get; set;}

        public string? Mab_NpcCards { get; set; } = "to be implemented";
    }
}
