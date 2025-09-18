namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsMabAddQuestRequest
    {
        public string? Mab_QuestTitle{ get; set; }

        public string? Mab_QuestDescription { get; set; }

        public int? Mab_QuestLevel { get; set; }

        public List<int>? Mab_NpcIds { get; set; }

        public int? Mab_GoldBounty { get; set; }

        public int? Mab_XpReward { get; set; } 
    }
}
