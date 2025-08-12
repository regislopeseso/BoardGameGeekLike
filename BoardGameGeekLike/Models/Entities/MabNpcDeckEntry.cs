using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("npcDeckEntries")]
    public class MabNpcDeckEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool IsDeleted { get; set; } = false;


        [ForeignKey("Card")]
        public int CardId { get; set; }
        public MabCard Card { get; set; }


        [ForeignKey("Npc")]
        public int NpcId { get; set; }
        [InverseProperty("Deck")]
        public MabNpc Npc { get; set; }
    }
}
