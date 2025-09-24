using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabShowCampaignStatisticsResponse
    {
        public bool? Mab_StartNewCampaign { get; set; } = true;

        public string? Mab_PlayerNickName { get; set; }

        public int? Mab_PlayerLevel { get; set; }

        public int? Mab_CurrentPlayerXp { get; set; }

        public int? Mab_NextPlayerLevelThreshold { get; set; }

        public MabCampaignDifficulty? Mab_CampaignDifficulty { get; set; }

        public int? Mab_Goldstash { get; set; }

        public int? Mab_QuestsCounts { get; set; }

        public int? Mab_FulfilledQuestsCount { get; set; }

        public int? Mab_BattlesCount { get; set; }

        public int? Mab_BattleVictoriesCount { get; set; }

        public int? Mab_BattleDefeatsCount { get; set; }

        public int? Mab_OpenedBoostersCount { get; set; }

        public int? Mab_CreatedDecksCount { get; set; }

        public bool? Mab_AllMabCardsCollectedTrophy { get; set; }

        public bool? Mab_AllMabNpcsDefeatedTrophy { get; set; }
    }

}
