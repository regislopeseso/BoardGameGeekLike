namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersActivateMabCardCopyRequest
    {
        public int? MabCardCopyId { get; set; }

        public int? ActiveMabDeckId { get; set; }
    }
}
