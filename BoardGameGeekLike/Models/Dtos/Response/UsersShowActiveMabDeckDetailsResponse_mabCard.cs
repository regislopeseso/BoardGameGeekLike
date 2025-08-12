using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersShowActiveMabDeckDetailsResponse_mabCard
    {
        public int? MabCardId { get; set; }

        public string? MabCardName { get; set; }

        public int MabCardPower { get; set; }

        public int MabCardUpperHand { get; set; }

        public int MabCardLevel { get; set; }

        public MabCardType MabCardType { get; set; }
    }
}
