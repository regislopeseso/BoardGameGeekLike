using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("battles")]
    public class Battle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? Winner { get; set; }

        public string? Results { get; set; }

        public bool IsFinished { get; set; } = false;


        [ForeignKey("Save")]
        public int SaveId { get; set; }
        public PlayerSave Save { get; set; }


        [ForeignKey("Npc")]
        public int NpcId { get; set; }
        public Npc Npc { get; set; }
    }
}
