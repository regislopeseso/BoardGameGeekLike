using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabStartDuelResponse
    {
        public bool? Mab_IsPlayerAttacking { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabPlayerState? Mab_PlayerState { get; set; } = MabPlayerState.Normal;
    }
}
