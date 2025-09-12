using BoardGameGeekLike.Models.Enums;
using BoardGameGeekLike.Utilities;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabContinueBattleResponse
    {
        public string? Mab_PlayerNickName { get; set; }

        public int? Mab_PlayerLevel { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabPlayerState? Mab_PlayerState { get; set; } 

        public List<UsersMabContinueBattleResponse_PlayerCard>? Mab_PlayerCards { get; set; }


        public string? Mab_NpcName { get; set; }

        public int? Mab_NpcLevel { get; set; }

        public List<UsersMabContinueBattleResponse_NpcCard>? Mab_NpcPlayedCards { get; set; }

        public int? Mab_DeckSize { get; set; } = Constants.DeckSize;

        public int? Mab_DuelsCount { get; set; } = null;

        public int? Mab_BattlePoints { get; set; } = null;

        public int? Mab_BattleEarnedXp { get; set; } = null;

        public int? Mab_BattleBonusXp { get; set; } = null;


        public bool? Mab_IsPlayerAttacking { get; set; } = null;

        public int? Mab_DuelPoints { get; set; } = null;

        public int? Mab_DuelEarnedXp { get; set; } = null;

        public int? Mab_DuelBonusXp { get; set; } = null;
    }
}
