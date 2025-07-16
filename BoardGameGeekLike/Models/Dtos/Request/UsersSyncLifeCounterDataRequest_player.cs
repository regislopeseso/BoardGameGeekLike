namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersSyncLifeCounterDataRequest_player
    {       
        public string? PlayerName { get; set; }

        public int? StartingLifePoints { get; set; }

        public int? CurrentLifePoints { get; set; }

        public bool FixedMaxLifePointsMode { get; set; }

        public int? MaxLifePoints { get; set; }

        public bool? AutoDefeatMode { get; set; }

        public bool IsDefeated { get; set; } = false;
    }
}
