using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsListMabCardsResponse
    {
        public int CardId { get; set; }

        public string? CardCode { get; set; }

        public string CardName { get; set; }

        public int CardPower { get; set; }

        public int CardUpperHand { get; set; }

        public int CardLevel { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabCardType CardType { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
