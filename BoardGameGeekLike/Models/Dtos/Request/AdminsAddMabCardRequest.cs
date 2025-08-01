using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsAddMabCardRequest
    {
        public string CardName { get; set; }

        public int CardPower { get; set; }

        public int CardUpperHand { get; set; }

        public MabCardType CardType { get; set; }
    }
}
