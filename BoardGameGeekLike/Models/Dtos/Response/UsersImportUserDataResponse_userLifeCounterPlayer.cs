namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersImportUserDataResponse_userLifeCounterPlayer
    {
        public string? LifeCounterManagerName { get; set; }

        public string? PlayerName { get; set; }

        public int? StartingLifePoints { get; set; }

        public int? CurrentLifePoints { get; set; }

        public bool? FixedMaxLifePointsMode { get; set; }

        public int? MaxLifePoints { get; set; }

        public bool? AutoDefeatMode { get; set; }

        public bool? IsDefeated { get; set; }
    }
}
