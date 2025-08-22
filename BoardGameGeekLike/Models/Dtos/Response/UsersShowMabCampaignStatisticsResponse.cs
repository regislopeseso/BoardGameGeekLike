using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersShowMabCampaignStatisticsResponse
    {
        public required string MabPlayerNickName { get; set; }

        public MabCampaignDifficulty MabCampaignDifficulty { get; set; }

        public int Goldstash { get; set; }

        public int CountMatches { get; set; }

        public int CountVictories { get; set; }

        public int CountDefeats { get; set; }

        public int CountBoosters { get; set; }

        public int PlayerLevel { get; set; }

        public int DecksOwned { get; set; }

        public bool AllCardsCollectedTrophy { get; set; }

        public bool AllNpcsDefeatedTrophy { get; set; }
    }

}
