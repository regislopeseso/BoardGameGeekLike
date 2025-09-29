using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabForgeCardResponse
    {
        public string? Mab_CardName { get; set; }

        public int? Mab_CardPower { get; set; }

        public int? Mab_CardUpperHand { get; set; }

        public MabCardType? Mab_CardType { get; set; }

        public string? Mab_CardCode { get; set; }
    }
}
