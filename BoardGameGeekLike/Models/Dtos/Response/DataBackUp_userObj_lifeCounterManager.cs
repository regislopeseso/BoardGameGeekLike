using BoardGameGeekLike.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class DataBackUp_userObj_lifeCounterManager
    {
        public string? LifeCounterTemplateName { get; set; }


        public string? LifeCounterManagerName { get; set; }

        public int? PlayersStartingLifePoints { get; set; }

        public int? PlayersCount { get; set; }

        public int? FirstPlayerIndex { get; set; }

        public bool? FixedMaxLifePointsMode { get; set; }

        public int? PlayersMaxLifePoints { get; set; }

        public bool? AutoDefeatMode { get; set; }

        public bool? AutoEndMode { get; set; }

        public long? StartingTime { get; set; }

        public long? EndingTime { get; set; }

        public double? Duration_minutes { get; set; }

        public bool? IsFinished { get; set; } = false;


        public List<int>? LifeCounterPlayerIds { get; set; }
             
    }
}
