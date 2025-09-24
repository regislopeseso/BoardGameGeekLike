namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabListQuestsResponse
    {
        public int? Mab_QuestId { get; set; }
        
        public string? Mab_QuestTitle { get; set; }

        public string? Mab_QuestDescription { get; set; }

        public int? Mab_GoldBounty { get; set; }

        public int? Mab_XpReward { get; set; }

        public int? Mab_NpcsCount { get; set; }

        public int? Mab_DefeatedNpcsCount { get; set; }

        public bool? Mab_IsQuestFulfilled { get; set; }
    }
}
