using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsGetAllCardsResponse
    {
        public int CardId { get; set; }
        public string CardName { get; set; }
        public int CardPower { get; set; }
        public int CardUpperHand { get; set; }
        public int CardLevel { get; set; }
        public CardType CardType { get; set; }
    }
}
