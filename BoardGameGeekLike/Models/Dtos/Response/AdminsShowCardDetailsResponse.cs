using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsShowCardDetailsResponse
    {
        public string? CardName { get; set; }

        public int? CardPower { get; set; }

        public int? CardUpperHand { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CardType? CardType { get; set; }

        public int? CardTypeValue { get {
                return (int?)CardType;
            } }
    }
}
