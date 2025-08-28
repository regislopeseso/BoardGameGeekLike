using BoardGameGeekLike.Models.Enums;

namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersMabStartCampaignRequest
    {       
        public string? MabPlayerNickName { get; set; }

        public MabCampaignDifficulty? MabCampaignDifficulty { get; set; }
    }
}
