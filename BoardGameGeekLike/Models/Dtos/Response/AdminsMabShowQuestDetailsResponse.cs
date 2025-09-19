namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsMabShowQuestDetailsResponse
    {
        public int? Mab_QuestId { get; set; }

        public string? Mab_QuestTitle { get; set; }

        public string? Mab_QuestDescription { get; set; }

        public int? Mab_QuestLevel { get; set; }

        public int? Mab_GoldBounty { get; set; }

        public int? Mab_XpReward { get; set; }

        public List<AdminsMabShowQuestDetailsResponse_Npc>? Mab_Npcs { get; set; }

    }
}
