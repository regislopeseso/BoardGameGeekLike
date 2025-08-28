using BoardGameGeekLike.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("MabCampaigns")]
    public class MabCampaign
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? Mab_PlayerNickname { get; set; }

        public int? Mab_PlayerLevel { get; set; } = 0;

        public MabCampaignDifficulty? Mab_Difficulty{ get; set; }

        public int? Mab_GoldStash { get; set; }

        public double? Mab_GoldValue { get; set; } = 1.0;

        public int? Mab_BattlesCount { get; set; } = 0;

        public int? Mab_BattleVictoriesCount { get; set; } = 0;

        public int? Mab_BattleDefeatsCount { get; set; } = 0;

        public int? Mab_OpenedBoostersCount { get; set; } = 0;

        public bool? Mab_AllCardsCollectedTrophy { get; set; } = false;

        public bool? Mab_AllNpcsDefeatedTrophy { get; set; } = false;

        public bool? Mab_IsCampaignDeleted { get; set; } = false;


        [ForeignKey(nameof(this.User))]
        public string? UserId { get; set; }
        public User? User { get; set; }

   
        [InverseProperty(nameof(MabDeck.Mab_Campaign))]
        public List<MabDeck>? Mab_Decks { get; set; }


        [InverseProperty(nameof(MabPlayerCard.Mab_Campaign))]
        public List<MabPlayerCard>? Mab_PlayerCards { get; set; }

        [InverseProperty(nameof(MabBattle.Mab_Campaign))]
        public List<MabBattle>? Mab_Battles { get; set; }
    }
}
