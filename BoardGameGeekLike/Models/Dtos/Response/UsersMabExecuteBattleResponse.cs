using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabExecuteBattleResponse
    {
        public int? Mab_BattleResult { get; set; } = null;

        public int? Mab_DuelResult { get; set; } = null;


        public int? Mab_PlayerCardFullPower { get; set; } = null;
        
        
        public string? Mab_NpcCardName { get; set; } = null;
        
        public int? Mab_NpcCardLevel { get; set; } = null;

        public int? Mab_NpcCardPower { get; set; } = null;

        public int? Mab_NpcCardUpperHand { get; set; } = null;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabCardType? Mab_NpcCardType { get; set; } = null;

        public int? Mab_NpcCardFullPower { get; set; } = null;
    }
}
