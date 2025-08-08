using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("decks")]
    public class Deck
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public required string Name { get; set; }
        
        public bool IsDeleted { get; set; } = false;

        
        [InverseProperty("Deck")]
        public List<PlayerDeckEntry> PlayerDeckEntries { get; set; }


        [InverseProperty("Decks")]
        public MedievalAutoBattlerCampaign MabCampaign { get; set; }
    }
}
