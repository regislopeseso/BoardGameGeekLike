using BoardGameGeekLike.Utilities;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsGetDeckSizeLimitResponse
    {
        public int? DeckSizeLimit { get; set; } = Constants.DeckSize;
    }
}
