using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabCheckDuelStatusResponse
    {
        public bool? IsPlayerTurn {  get; set; }

        public int? Mab_PlayerCardFullPower { get; set; } = null;

        public MabPlayerState? Mab_PlayerState { get; set; } = null;


        public int? Mab_NpcCardFullPower { get; set; } = null;


        public bool? Mab_IsDuelFinished { get; set; } = false;

        public int? Mab_DuelPoints { get; set; } = 0;

        public int? Mab_EarnedExp { get; set; } = 0;


        public bool? Mab_IsBattleFinished { get; set; } = false;

        public int? Mab_BattlePoints { get; set; } = 0;
    }
}
