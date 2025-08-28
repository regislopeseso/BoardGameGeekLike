using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("MabPlayerCards")]
    public class MabPlayerCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [ForeignKey(nameof(this.Mab_Card))]
        public int? Mab_CardId { get; set; }
        [InverseProperty(nameof(MabCard.Mab_PlayerCards))]
        public MabCard? Mab_Card { get; set; }


        [ForeignKey(nameof(this.Mab_Campaign))]
        public int? Mab_CampaignId { get; set; }
        [InverseProperty(nameof(MabCampaign.Mab_PlayerCards))]
        public MabCampaign? Mab_Campaign { get; set; }


        [InverseProperty(nameof(MabAssignedCard.Mab_PlayerCard))]
        public List<MabAssignedCard>? Mab_AssignedCards { get; set; }
    }
}
     