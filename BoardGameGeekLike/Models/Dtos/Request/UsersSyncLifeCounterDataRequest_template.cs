namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersSyncLifeCounterDataRequest_template
    {
        public string? LifeCounterTemplateName { get; set; }

        public int? PlayersStartingLifePoints { get; set; }

        public int? PlayersCount { get; set; }

        public bool? FixedMaxLifePointsMode { get; set; }

        public int? PlayersMaxLifePoints { get; set; }

        public bool? AutoDefeatMode { get; set; }

        public bool? AutoEndMode { get; set; }

        public int? LifeCounterManagersCount { get; set; }


        public List<UsersSyncLifeCounterDataRequest_manager>? LifeCounterManagers { get; set; }
    }
}
