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

        public int? Mab_PlayerExperience { get; set; } = 0;

        public MabCampaignDifficulty? Mab_Difficulty{ get; set; }

        public int? Mab_CoinsStash { get; set; } = 0;

        public int? Mab_BattlesCount { get; set; } = 0;

        public int? Mab_BattleVictoriesCount { get; set; } = 0;

        public int? Mab_BattleDefeatsCount { get; set; } = 0;

        public int? Mab_OpenedBoostersCount { get; set; } = 0;


        public int? Mab_ForgingsCount {  get; set; } = 0;

        public int? Mab_SharpenCount { get; set; } = 0;

        public int? Mab_MeltCount { get; set; } = 0;

        public int? Mab_CountForgings { get; set; } = 0;

        public int? Mab_BrassStash { get; set; } = 0;
        public int? Mab_BrassInflation { get; set; } = 0;


        public int? Mab_CopperStash { get; set; } = 0;
        public int? Mab_CopperInflation { get; set; } = 0;

        public int? Mab_IronStash { get; set; } = 0;
        public int? Mab_IronInflation { get; set; } = 0;

        public int? Mab_SteelStash { get; set; } = 0;
        public int? Mab_SteelInflation { get; set; } = 0;

        public int? Mab_TitaniumStash { get; set; } = 0;
        public int? Mab_TitaniumInflation { get; set; } = 0;

        public int? Mab_SilverStash { get; set; } = 0;
        public int? Mab_SilverInflation { get; set; } = 0;

        public int? Mab_GoldStash { get; set; } = 0;
        public int? Mab_GoldInflation { get; set; } = 0;

        public int? Mab_DiamondStash { get; set; } = 0;
        public int? Mab_DiamondInflation { get; set; } = 0;

        public int? Mab_AdamantiumStash { get; set; } = 0;
        public int? Mab_AdamantiumInflation { get; set; } = 0;

        public bool? Mab_TheCollectorTrophy { get; set; } = false;

        public bool? Mab_TheBraveTrophy { get; set; } = false;

        public bool? Mab_TheBourgeoisTrophy { get; set; } = false;

        public bool? Mab_TheMinerTrophy { get; set; } = false;

        public bool? Mab_TheBlacksmithTrophy { get; set; } = false;


        public bool? Mab_IsCampaignDeleted { get; set; } = false;



        [ForeignKey(nameof(this.User))]
        public string? UserId { get; set; }
        [InverseProperty(nameof(User.MabCampaigns))]
        public User? User { get; set; }

   
        [InverseProperty(nameof(MabDeck.Mab_Campaign))]
        public List<MabDeck>? Mab_Decks { get; set; }


        [InverseProperty(nameof(MabPlayerCard.Mab_Campaign))]
        public List<MabPlayerCard>? Mab_PlayerCards { get; set; }


        [InverseProperty(nameof(MabBattle.Mab_Campaign))]
        public List<MabBattle>? Mab_Battles { get; set; }
            
    }
}
