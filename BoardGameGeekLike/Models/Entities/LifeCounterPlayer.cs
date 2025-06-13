using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("LifeCounterPlayers")]
    public class LifeCounterPlayer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? PlayerName { get; set; }

        public int? StartingLifePoints { get; set; }

        public int? CurrentLifePoints { get; set; }

        public bool FixedMaxLifePointsMode { get; set; }
        public int? MaxLifePoints { get; set; }

        [ForeignKey(nameof(LifeCounterManager))]       
        public int LifeCounterManagerId { get; set; }

        public LifeCounterManager? LifeCounterManager { get; set; }


        public bool IsDefeated { get; set; } = false;
    }
}
