using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabListPlayerDuellingCardsResponse
    {
        public int? Mab_PlayerCardId { get; set; }

        public string? Mab_CardCode { get; set; }

        public string? Mab_CardName { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabCardType? Mab_CardType { get; set; }

        public int? Mab_CardLevel { get; set; }

        public int? Mab_CardPower { get; set; }

        public int? Mab_CardUpperHand { get; set; }


        public int? Mab_CardFullPower { get; set; } = null;


        public int? Mab_DuelId { get; set; } = null;

        public int? Mab_DuelPoints { get; set; } = null;

        public int? Mab_DuelEarnedXp { get; set; } = null;

        public int? Mab_DuelBonusXp { get; set; } = null;
    }
}
