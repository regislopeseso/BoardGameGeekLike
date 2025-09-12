using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabListAssignedCardsResponse
    {
        public int? Mab_DuelId { get; set; } = null;

        public int? Mab_PlayerCardId { get; set; }

        public string? Mab_CardName { get; set; }

        public int? Mab_CardLevel { get; set; }

        public int? Mab_CardPower { get; set; }

        public int? Mab_CardUpperHand { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabCardType? Mab_CardType { get; set; }

        public int? Mab_CardFullPower { get; set; } = null;



        public bool? Mab_IsCardAvailable { get; set; }

        public bool? Mab_HasPlayerWon { get; set; } = null;

        public int? Mab_DuelPoints { get; set; } = null;

        public int? Mab_EarnedXp { get; set; } = null;

        public int? Mab_BonusXp { get; set; } = null;
    }
}
