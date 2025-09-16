using BoardGameGeekLike.Models.Enums;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("MabCards")]
    public class MabCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? Mab_CardCode { get; set; }

        public string Mab_CardName { get; set; }

        public int Mab_CardPower { get; set; }

        public int Mab_CardUpperHand { get; set; }

        public int Mab_CardLevel { get; set; }

        public MabCardType Mab_CardType { get; set; }

        public bool Mab_IsCardDeleted { get; set; } = false;

        public bool Mab_IsCardDummy { get; set; } = false;


        [InverseProperty(nameof(MabPlayerCard.Mab_Card))]
        public List<MabPlayerCard>? Mab_PlayerCards { get; set; }

        [InverseProperty(nameof(MabPlayerCard.Mab_Card))]
        public List<MabNpcCard>? Mab_NpcCards { get; set; }

    }
}
