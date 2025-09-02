using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersMabResolveDuelRequest
    {
        public int? Mab_CardPower_player {  get; set; }

        public int? Mab_CardUpperHand_player { get; set; }

        public MabCardType? Mab_CardType_player { get; set; }


        public int? Mab_CardPower_npc { get; set; }

        public int? Mab_CardUpperHand_npc { get; set; }

        public MabCardType? Mab_CardType_npc { get; set; }
    }
}
