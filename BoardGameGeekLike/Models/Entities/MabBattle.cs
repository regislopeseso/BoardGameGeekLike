using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("MabBattles")]
    public class MabBattle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public bool? Mab_IsPlayerTurn { get; set; }

        public int? Mab_BattlePoints { get; set; }

        public bool? Mab_HasPlayerWon { get; set; }
        
        public bool Mab_IsBattleFinished { get; set; } = false;

        public string? Mab_BattleResults { get; set; }


        [ForeignKey(nameof(this.Mab_Campaign))]
        public int Mab_CampaignId { get; set; }
        public MabCampaign Mab_Campaign { get; set; }


        [ForeignKey(nameof(this.Mab_Npc))]
        public int Mab_NpcId { get; set; }
        public MabNpc Mab_Npc { get; set; }


        [InverseProperty(nameof(MabDuel.Mab_Battle))]
        public List<MabDuel>? Mab_Duels { get; set; }
    }
}
