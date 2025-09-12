using BoardGameGeekLike.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("MabDuels")]
    public class MabDuel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }       

        public int? Mab_PlayerCardId { get; set; } = null;

        public int? Mab_PlayerCardFullPower { get; set; } = null;

        public MabPlayerState? Mab_PlayerState { get; set; } = MabPlayerState.Normal;

        public bool? Mab_IsPlayerAttacking { get; set; }

        public int? Mab_NpcCardId { get; set; } = null;

        public int? Mab_NpcCardFullPower { get; set; } = null;

        public bool? Mab_HasPlayerWon { get; set; } = null;
        
        public int? Mab_DuelPoints { get; set; } = null;

        public int? Mab_EarnedXp { get; set; } = null; 

        public int? Mab_BonusXp { get; set; } = null;

        [ForeignKey(nameof(this.Mab_Battle))]
        public int? Mab_BattleId { get; set; }
        [InverseProperty(nameof(MabBattle.Mab_Duels))]
        public MabBattle? Mab_Battle { get; set; }
    }
}
