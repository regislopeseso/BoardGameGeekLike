using BoardGameGeekLike.Models.Entities;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetLifeCounterPlayersDetailsResponse
    {
        public List<UsersGetLifeCounterPlayersDetailsResponse_players>? LifeCounterPlayers { get; set; }
    }
}
