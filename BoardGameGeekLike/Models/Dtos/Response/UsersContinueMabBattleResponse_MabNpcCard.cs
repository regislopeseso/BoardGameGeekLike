using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersContinueMabBattleResponse_MabNpcCard
    {
        public int? MabNpcCardId { get; set; }
        public string? MabCardName { get; set; }

        public int? MabCardLevel { get; set; }       

        public int? MabCardPower { get; set; }

        public int? MabCardUpperHand { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabCardType? MabCardType { get; set; }
    }
}
