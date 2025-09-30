using BoardGameGeekLike.Models.Enums;
using System.Text.Json.Serialization;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabMeltCardResponse
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MabRawMaterialType?  Mab_RawMaterialType { get; set; }

        public int? Mab_ExtractedRawMaterial { get; set; }

        public int? Mab_GainedXp { get; set; }
    }
}
