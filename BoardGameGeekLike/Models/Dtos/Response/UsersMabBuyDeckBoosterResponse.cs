using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabBuyDeckBoosterResponse
    {
        public string? Mab_CardName { get; set; }

        public string? Mab_CardCode { get; set; }

        public MabCardType? MabCardType { get; set; }   

        public int? Mab_CardLevel { get; set; }

        public int? Mab_CardPower { get; set; }

        public int? Mab_CardUpperHand { get; set; }
    }
}
