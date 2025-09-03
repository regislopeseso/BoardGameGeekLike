using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabAutoBattleResponse_playerCard
    {
        public string? Mab_CardName { get; set; }

        public int? Mab_CardPower { get; set; }

        public int? Mab_CardUpperHand { get; set; }

        public int? Mab_CardLevel { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabCardType Mab_CardType { get; set; }

        public int? Mab_CardFullPower { get; set; }
    }
}
