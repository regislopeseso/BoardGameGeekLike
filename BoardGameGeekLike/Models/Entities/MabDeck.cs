using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("MabDecks")]
    public class MabDeck
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? Mab_DeckName { get; set; }

        public bool? Mab_IsDeckActive { get; set; }     


        [ForeignKey(nameof(this.Mab_Campaign))]
        public int? Mab_CampaignId { get; set; }
        [InverseProperty(nameof(MabCampaign.Mab_Decks))]
        public MabCampaign? Mab_Campaign { get; set; }


        [InverseProperty(nameof(MabAssignedCard.Mab_Deck))]
        public List<MabAssignedCard>? Mab_PlayerCards { get; set; }
    }
}
