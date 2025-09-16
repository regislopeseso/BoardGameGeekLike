using BoardGameGeekLike.Models.Enums;
using BoardGameGeekLike.Utilities;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabStartBattleResponse
    {
        public int? Mab_BattleId { get; set; }

        public string? Mab_PlayerNickName { get; set; }

        public int? Mab_PlayerLevel { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabPlayerState? Mab_PlayerState { get; set; } = MabPlayerState.Normal;

        public bool? Mab_DoesPlayerGoFirst { get; set; }

        public string? Mab_NpcName { get; set; }    
        
        public int? Mab_NpcLevel { get; set; }


        public int? Mab_DeckSize { get; set; } = Constants.DeckSize;
    }
}
