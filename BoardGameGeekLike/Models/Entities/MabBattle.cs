using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("mabBattles")]
    public class MabBattle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? Winner { get; set; }

        public string? Results { get; set; }

        public bool IsFinished { get; set; } = false;


        [ForeignKey("MedievalAutoBattlerCampaign")]
        public int MabCampaignId { get; set; }
        public MabCampaign MabCampaign { get; set; }


        [ForeignKey("Npc")]
        public int NpcId { get; set; }
        public MabNpc Npc { get; set; }
    }
}
