using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("LifeCounterTemplates")]
    public class LifeCounterTemplate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? LifeCounterTemplateName { get; set; }

        public int? PlayersStartingLifePoints { get; set; }

        public int? PlayersCount { get; set; } 

        public bool? FixedMaxLifePointsMode { get; set; }

        public int? PlayersMaxLifePoints { get; set; }

        public bool? AutoDefeatMode { get; set; }

        public bool? AutoEndMode { get; set; } 

        public int? LifeCounterManagersCount { get; set; } = 0;

        public List<LifeCounterManager>? LifeCounterManagers { get; set; }


        [ForeignKey(nameof(User))]
        public string? UserId { get; set; }

        public User? User { get; set; }
    }
}
