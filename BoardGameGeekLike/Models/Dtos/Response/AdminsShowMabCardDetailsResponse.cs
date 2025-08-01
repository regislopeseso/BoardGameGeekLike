using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsShowMabCardDetailsResponse
    {
        public string? CardName { get; set; }

        public int? CardPower { get; set; }

        public int? CardUpperHand { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabCardType? CardType { get; set; }

        public int? CardTypeValue { get {
                return (int?)CardType;
            } }
    }
}
