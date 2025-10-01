using BoardGameGeekLike.Utilities;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabShowMiningDetailsResponse
    {
        public int? Mab_MiningPrice { get; set; } = Constants.BaseMiningPrice;

        public string? Mab_CardName { get; set; }

        public int? Mab_CardPower { get; set; }

        public int? Mab_CardUpperHand { get; set; }

        public string? Mab_CardCode { get; set; }
    }
}
