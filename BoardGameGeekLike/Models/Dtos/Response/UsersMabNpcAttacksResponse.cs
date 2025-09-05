using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabNpcAttacksResponse
    {     
        public int? Mab_CardId { get; set; }

        public int? Mab_NpcCardId { get; set; }

        //apagar os 2 acima...

        public string? Mab_CardName {  get; set; }

        public int? Mab_CardLevel { get; set; }

        public int? Mab_CardPower { get; set; }

        public int? Mab_CardUpperHand { get; set;}

        public MabCardType Mab_CardType { get; set; }
    }
}
