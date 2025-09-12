using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabListNpcPlayedCardsResponse
    {
        public int? Mab_BattleId { get; set; }

        public int? Mab_NpcCardId { get; set; }

        public string? Mab_CardName { get; set; }

        public int? Mab_CardLevel { get; set; }

        public int? Mab_CardPower { get; set; }

        public int? Mab_CardUpperHand { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabCardType? Mab_CardType { get; set; }

        public int? Mab_CardFullPower { get; set; }
    }
}
