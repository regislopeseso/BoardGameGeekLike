using BoardGameGeekLike.Models.Entities;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersGetSessionsResponse
    {
        public List<Session>? Sessions { get; set; }
    }
}
