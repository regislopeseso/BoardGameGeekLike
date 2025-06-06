using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("LifeCounterManagers")]
    public class LifeCounterManager
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("LifeCounter")]
        public int? LifeCounterId { get; set; }
     
        public LifeCounter? LifeCounter { get; set; }

        public List<LifeCounterPlayer>? LifeCounterPlayers { get; set; }

        public int? PlayersCount { get; set; }

        public long? StartingTime { get; set; }

        public long? EndingTime { get; set; }

        public int? Duration_minutes { get; set; }

        public bool AutoEnd { get; set; }

        public bool IsFinished { get; set; } = false;
    }
}
