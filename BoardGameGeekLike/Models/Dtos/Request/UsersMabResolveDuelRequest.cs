using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersMabResolveDuelRequest
    {
        public int? UserId { get; set; }

        public int? Mab_PlayerCardId {  get; set; }

        public int? MabNpcCardId { get; set; }
    }
}
