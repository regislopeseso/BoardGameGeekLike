using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsEditCardRequest
    {
        public int CardId { get; set; }
        
        public string CardName { get; set; }
        
        public int CardPower { get; set; }

        public int CardUpperHand { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CardType CardType { get; set; }
    }
}
