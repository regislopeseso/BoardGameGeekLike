using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class ExploreShowBoardGameDetailsResponse_sessions
    {
        public int? SessionId { get; set; }

        public string? UserNickName {get; set;}

        public DateOnly? Date {get; set;}

        public int PlayersCount {get; set;}

        public int? Duration {get; set;}
    }
}