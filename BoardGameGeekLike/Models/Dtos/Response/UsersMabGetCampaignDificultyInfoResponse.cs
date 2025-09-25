namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabGetCampaignDificultyInfoResponse
    {
        public int? Mab_StartingGoldStash { get; set; }

        public int? Mab_StartingCardsMaxLevel { get; set; }

        public int? Mab_StartingCardsCount { get; set; }

        public int? Mab_QuestsBaseGoldBounty { get; set; }

        public int? Mab_QuestsBaseXpReward { get; set; }
    }
}
