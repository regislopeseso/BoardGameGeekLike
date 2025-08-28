using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("MabAssignedCards")]
    public class MabAssignedCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [ForeignKey(nameof(this.Mab_PlayerCard))]
        public int? Mab_PlayerCardId { get; set; }
        [InverseProperty(nameof(MabPlayerCard.Mab_AssignedCards))]
        public MabPlayerCard? Mab_PlayerCard { get; set; }


        [ForeignKey(nameof(this.Mab_Deck))]
        public int? Mab_DeckId { get; set; }
        [InverseProperty(nameof(MabDeck.Mab_PlayerCards))]
        public MabDeck? Mab_Deck { get; set; }

    }
}
