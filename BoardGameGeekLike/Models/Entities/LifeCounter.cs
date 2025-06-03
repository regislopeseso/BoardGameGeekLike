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

        public required string Name { get; set; }

        public required int StartingLifePoints { get; set; }

        public required bool FixedMaxLife { get; set; }

        public required bool AutoEndMatch { get; set; }

        [ForeignKey(nameof(User))]
        public string? UserId { get; set; }

        public User? User { get; set; }
    }
}
