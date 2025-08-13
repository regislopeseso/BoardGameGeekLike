using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("mabPlayerCardCopy")]
    public class MabPlayerCardCopy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool IsActive { get; set; } = false;

        public bool IsDeleted { get; set; } = false;


        [ForeignKey("MabCard")]
        public int? MabCardId { get; set; }
        [InverseProperty("MabPlayerCardCopies")]
        public MabCard? MabCard { get; set; }


        [ForeignKey("MabCampaign")]
        public int? MabCampaignId { get; set; }
        [InverseProperty("MabPlayerCardCopies")]
        public MabCampaign? MabCampaign { get; set; }
        

        [ForeignKey("MabPlayerDeck")]
        public int? MabPlayerDeckId { get; set; }
        [InverseProperty("MabPlayerCardCopies")]
        public MabPlayerDeck? MabPlayerDeck { get; set; }
    }
}
     