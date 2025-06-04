using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("LifeCounters")]
    public class LifeCounter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? Name { get; set; }

        public int? StartingLifePoints { get; set; }

        public int? MaxLifePoints { get; set; } 

        public bool FixedMaxLife { get; set; }

        public bool AutoEndMatch { get; set; }

        [ForeignKey(nameof(User))]
        public string? UserId { get; set; }

        public User? User { get; set; }

        public List<LifeCounterManager>? LifeCounterManagerInstances { get; set; }
    }
}
