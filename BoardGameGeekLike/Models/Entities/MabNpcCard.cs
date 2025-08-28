using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("MabNpcCards")]
    public class MabNpcCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool Mab_IsNpcCardDeleted { get; set; } = false;


        [ForeignKey(nameof(this.Mab_Card))]
        public int Mab_CardId { get; set; }
        [InverseProperty(nameof(MabCard.Mab_NpcCards))]
        public MabCard Mab_Card { get; set; }


        [ForeignKey(nameof(this.Mab_Npc))]
        public int Mab_NpcId { get; set; }
        [InverseProperty(nameof(MabNpc.Mab_NpcCards))]
        public MabNpc Mab_Npc { get; set; }
    }
}
