using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsCreateCardRequest
    {
        public string CardName { get; set; }

        public int CardPower { get; set; }

        public int CardUpperHand { get; set; }

        public CardType CardType { get; set; }
    }
}
