using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabExecuteBattleResponse
    {
        public int? Mab_BattleResult { get; set; } = null;

        public int? Mab_DuelResult { get; set; } = 0;

        
        public string? Mab_NpcCardName { get; set; } = null;
        
        public int? Mab_NpcCardLevel { get; set; } = null;

        public int? Mab_NpcCardPower { get; set; } = null;

        public int? Mab_NpcCardUpperHand { get; set; } = null;

        public MabCardType? Mab_NpcCardType { get; set; } = null;
    }
}
