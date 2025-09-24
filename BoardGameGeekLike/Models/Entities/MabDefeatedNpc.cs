using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("MabDefeatedNpcs")]
    public class MabDefeatedNpc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? Mab_CampaignId { get; set; }

        public int? Mab_NpcId { get; set; }

        public int? Mab_QuestId { get; set; }
    }
}
