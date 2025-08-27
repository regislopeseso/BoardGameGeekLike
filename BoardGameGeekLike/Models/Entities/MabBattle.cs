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

        public int? MabBattlePoints { get; set; }

        public bool? IsPlayerTurn { get; set; }

        public bool? HasPlayerWon { get; set; }

        public string? Results { get; set; }

        public bool IsFinished { get; set; } = false;


        [ForeignKey("MabCampaign")]
        public int MabCampaignId { get; set; }
        public MabPlayerCampaign MabPlayerCampaign { get; set; }


        [ForeignKey("Npc")]
        public int NpcId { get; set; }
        public MabNpc Npc { get; set; }

        [InverseProperty("MabBattle")]
        public List<MabBattleTurn>? MabBattleTurns { get; set; }
    }
}
