namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsMabEditQuestRequest
    {
        public int? Mab_QuestId { get; set; }

        public string? Mab_QuestTitle { get; set; }

        public string? Mab_QuestDescription { get; set; }

        public int? Mab_QuestLevel { get; set; }

        public List<int>? Mab_NpcIds { get; set; }

        public int? Mab_GoldBounty { get; set; }

        public int? Mab_XpReward { get; set; }
    }
}
