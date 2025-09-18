using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("MabFulfilledQuests")]
    public class MabFulfilledQuest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [ForeignKey(nameof(this.Mab_Quest))]
        public int? Mab_QuestId { get; set; }
        [InverseProperty(nameof(MabQuest.Mab_FulfilledQuests))]
        public MabQuest? Mab_Quest { get; set; }


        [ForeignKey(nameof(this.Mab_Campaign))]
        public int? Mab_CampaignId { get; set; }
        [InverseProperty(nameof(MabCampaign.Mab_FulfilledQuests))]
        public MabCampaign? Mab_Campaign { get; set; }    
    }
}
