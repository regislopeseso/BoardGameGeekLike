using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("MabNpcs")]
    public class MabNpc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public required string Mab_NpcName { get; set; }

        public required string Mab_NpcDescription { get; set; }

        public int Mab_NpcLevel { get; set; }

        public bool Mab_IsNpcDeleted { get; set; } = false;

        public bool Mab_IsNpcDummy { get; set; } = false;


        [InverseProperty(nameof(MabNpcCard.Mab_Npc))]
        public List<MabNpcCard> Mab_NpcCards { get; set; }
    }
}
