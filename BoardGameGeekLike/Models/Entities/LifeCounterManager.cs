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

        public string? LifeCounterManagerName { get; set; }          
        
        public int? PlayersCount { get; set; }

        public int? FirstPlayerIndex {get; set; }

        public int? PlayersStartingLifePoints { get; set; }

        public bool? FixedMaxLifePointsMode { get; set; }

        public int? PlayersMaxLifePoints { get; set; }

        public bool? AutoDefeatMode { get; set; }

        public bool? AutoEndMode { get; set; }

        public long? StartingTime { get; set; }

        public long? EndingTime { get; set; }

        public double? Duration_minutes { get; set; }

        public bool IsFinished { get; set; } = false;


        public List<LifeCounterPlayer>? LifeCounterPlayers { get; set; }


        [ForeignKey(nameof(User))]
        public string? UserId { get; set; }
        public User? User { get; set; }


        [ForeignKey(nameof(LifeCounterTemplate))]
        public int? LifeCounterTemplateId { get; set; }
        public LifeCounterTemplate? LifeCounterTemplate { get; set; }
    }
}
