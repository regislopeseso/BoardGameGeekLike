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

        public bool IsDeleted { get; set; } = false;


        [ForeignKey("MabCard")]
        public int? MabCardId { get; set; }
        [InverseProperty("MabPlayerCardCopies")]
        public MabCard? MabCard { get; set; }


        [ForeignKey("MabPlayerCampaign")]
        public int? MabPlayerCampaignId { get; set; }
        [InverseProperty("MabPlayerCardCopies")]
        public MabPlayerCampaign? MabPlayerCampaign { get; set; }

     
        public List<MabPlayerAssignedCardCopy>? MabPlayerAssignedCardCopies { get; set; }
    }
}
     