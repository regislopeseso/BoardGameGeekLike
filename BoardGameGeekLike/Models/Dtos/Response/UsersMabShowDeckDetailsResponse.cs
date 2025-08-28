using BoardGameGeekLike.Utilities;
using System.Reflection.Metadata;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabShowDeckDetailsResponse
    {
        public int? Mab_DeckId { get; set; }

        public string? Mab_DeckName { get; set; }

        public int? Mab_DeckLevel { get; set; }

        public int Mab_DeckSizeLimit { get; set; } = Constants.DeckSize;
        
        public List<UsersMabShowDeckDetailsResponse_assignedCard>? Mab_AssignedCards { get; set; }
    }
}
