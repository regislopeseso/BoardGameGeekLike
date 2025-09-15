using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabFinishBattleResponse
    {
        public bool? Mab_HasPlayerWon { get; set; } 

        public int? Mab_BattlePoints { get; set; } 
        public int? Mab_BattleEarnedXp { get; set; }
        public int? Mab_BattleBonusXp { get; set; }

        public int? Mab_UpdatedGoldStash { get; set; }
        public int? Mab_UpdatedPlayerXp { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabPlayerState? Mab_PlayerState { get; set; }
    }
}
