using BoardGameGeekLike.Models.Enums;
using BoardGameGeekLike.Utilities;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabPlayerRetreatsResponse
    {
        public bool? Mab_HasPlayerWon { get; set; } = false;

        public int? Mab_EarnedGold { get; set; } = Constants.RetreatGoldPenalty;

        public int? Mab_UpdatedGoldStash { get; set; }

        public int? Mab_EarnedXp { get; set; } = 0;

        public int? Mab_UpdatedPlayerXp { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabPlayerState? Mab_PlayerState {  get; set; } = MabPlayerState.Panicking;
    }
}
