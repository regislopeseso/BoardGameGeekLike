using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("mabPlayerDecks")]
    public class MabPlayerDeck
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? Name { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsDeleted { get; set; } = false;




        [ForeignKey("MabPlayerCampaign")]
        public int? MabPlayerCampaignId { get; set; }
        [InverseProperty("MabPlayerDecks")]
        public MabPlayerCampaign? MabPlayerCampaign { get; set; }


        [InverseProperty("MabPlayerDeck")]
        public List<MabPlayerAssignedCardCopy>? MabPlayerAssignedCardCopies { get; set; }
    }
}
