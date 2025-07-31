using BoardGameGeekLike.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class DataBackUp_userObj_lifeCounterPlayer
    {
        public string? LifeCounterManagerName { get; set; }

        public string? PlayerName { get; set; }

        public int? StartingLifePoints { get; set; }

        public int? CurrentLifePoints { get; set; }

        public bool? FixedMaxLifePointsMode { get; set; }

        public int? MaxLifePoints { get; set; }

        public bool? AutoDefeatMode { get; set; }

        public bool? IsDefeated { get; set; } = false;
    }
}
