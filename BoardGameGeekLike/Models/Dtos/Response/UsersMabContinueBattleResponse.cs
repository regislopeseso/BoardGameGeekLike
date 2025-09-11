using BoardGameGeekLike.Utilities;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabContinueBattleResponse
    {
        public string? Mab_PlayerNickName { get; set; }

        public int? Mab_PlayerLevel { get; set; }

        public UsersMabContinueBattleResponse_PlayerCard? Mab_PlayerCard { get; set; }


        public string? Mab_NpcName { get; set; }

        public int? Mab_NpcLevel { get; set; }

        public UsersMabContinueBattleResponse_NpcCard? Mab_NpcCard { get; set; }

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
