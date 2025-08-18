using BoardGameGeekLike.Utilities;
using System.Reflection.Metadata;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersShowMabPlayerDeckDetailsResponse
    {
        public int? ActiveMabDeckId { get; set; }

        public string? ActiveMabDeckName { get; set; }

        public int MabDeckSizeLimit { get; set; } = Constants.DeckSize;
        
        public List<UsersShowMabPlayerDeckDetailsResponse_mabCardCopy>? MabCardCopies { get; set; }
    }
}
