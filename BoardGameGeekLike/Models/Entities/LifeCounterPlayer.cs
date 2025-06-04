using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace BoardGameGeekLike.Models.Entities
{
    public class LifeCounterPlayer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? StartingLife { get; set; }

        public int? CurrentLife { get; set; }

        public int? MaxLife { get; set; }

        [ForeignKey("LifeCounterManager")]
        public int LifeCounterManagerId { get; set; }
        public LifeCounterManager? LifeCounterManager { get; set; }

        public bool FixedMaxLife { get; set; }

        public bool IsDefeated { get; set; } = false;
    }
}
