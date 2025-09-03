using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabAutoBattleResponse_duelResult
    {
        public int? Mab_DuelPoints { get; set; }

        public int? Mab_EarnedXp { get; set; }     

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabPlayerState? Mab_PlayerState { get; set; }
    }
}
