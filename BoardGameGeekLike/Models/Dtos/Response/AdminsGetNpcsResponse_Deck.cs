using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsGetNpcsResponse_Deck
    {       
        public string Name { get; set; }

        public int Power { get; set; }

        public int UpperHand { get; set; }

        public int Level { get; set; }


        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabCardType Type { get; set; }
    }
}
