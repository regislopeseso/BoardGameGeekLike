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

        public string? LifeCounterName { get; set; }

        [ForeignKey(nameof(User))]
        public string? UserId { get; set; }
        public User? User { get; set; }

        [ForeignKey(nameof(LifeCounterTemplate))]
        public int? LifeCounterTemplateId { get; set; }
        public LifeCounterTemplate? LifeCounterTemplate { get; set; }

         

        public List<LifeCounterPlayer>? LifeCounterPlayers { get; set; }

        public int? PlayersCount { get; set; }

        public long? StartingTime { get; set; }

        public long? EndingTime { get; set; }

        public int? Duration_minutes { get; set; }

        public bool? FixedMaxLifePointsMode { get; set; } = false;
        public int? PlayersMaxLifePoints { get; set; }

        public bool AutoEndMode { get; set; }

        public bool IsFinished { get; set; } = false;
    }
}
