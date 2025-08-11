using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersStartMabCampaignRequest
    {
        public string? UserId { get; set; }

        public string? MabPlayerNickName { get; set; }

        public MabCampaignDifficulty MabCampaignDifficulty { get; set; }
    }
}
