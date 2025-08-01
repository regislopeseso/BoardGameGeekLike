using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsEditMabCardRequest
    {
        public int CardId { get; set; }
        
        public string CardName { get; set; }
        
        public int CardPower { get; set; }

        public int CardUpperHand { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabCardType CardType { get; set; }
    }
}
