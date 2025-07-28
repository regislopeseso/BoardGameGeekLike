using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("npcs")]
    public class Npc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public List<NpcDeckEntry> Deck { get; set; }
        public int Level { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsDummy { get; set; } = false;
    }
}
