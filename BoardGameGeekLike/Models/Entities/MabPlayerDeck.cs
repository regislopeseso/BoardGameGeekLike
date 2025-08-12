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




        [ForeignKey("MabCampaign")]
        public int? MabCampaignId { get; set; }
        [InverseProperty("MabPlayerDecks")]
        public MabCampaign? MabCampaign { get; set; }


        [InverseProperty("MabPlayerDeck")]
        public List<MabPlayerCardCopy>? MabPlayerCardCopies { get; set; }
    }
}
