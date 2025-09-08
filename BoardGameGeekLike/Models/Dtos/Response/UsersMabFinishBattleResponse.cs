using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabFinishBattleResponse
    {
        public bool? Mab_HasPlayerWon { get; set; } 

        public int? Mab_EarnedGold { get; set; } 
        public int? Mab_UpdatedGoldStash { get; set; }

        public int? Mab_EarnedXp { get; set; }
        public int? Mab_BonustXp { get; set; }

        public int? Mab_UpdatedPlayerXp { get; set; }

        public int? Mab_Bonus { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabPlayerState? Mab_PlayerState { get; set; }
    }
}
