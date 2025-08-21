using BoardGameGeekLike.Utilities;
using System.Reflection.Metadata;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersShowMabDeckDetailsResponse
    {
        public int? ActiveMabDeckId { get; set; }

        public string? ActiveMabDeckName { get; set; }

        public int? DeckLevel { get; set; }

        public int MabDeckSizeLimit { get; set; } = Constants.DeckSize;
        
        public List<UsersShowMabDeckDetailsResponse_assignedCardCopy>? MabCardCopies { get; set; }
    }
}
