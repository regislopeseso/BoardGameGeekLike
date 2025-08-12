using BoardGameGeekLike.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("mabCampaigns")]
    public class MabCampaign
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? MabPlayerNickName { get; set; }

        public int? MabPlayerLevel { get; set; } = 0;

        public MabCampaignDifficulty? Difficulty{ get; set; }

        public int? GoldStash { get; set; }

        public int? CountMatches { get; set; } = 0;

        public int? CountVictories { get; set; } = 0;

        public int? CountDefeats { get; set; } = 0;

        public int? CountBoosters { get; set; } = 0;

        public bool? AllCardsCollectedTrophy { get; set; } = false;

        public bool? AllNpcsDefeatedTrophy { get; set; } = false;

        public bool? IsDeleted { get; set; } = false;


        [ForeignKey("User")]
        public string? UserId { get; set; }
        public User? User { get; set; }

   
        [InverseProperty("MabCampaign")]
        public List<MabPlayerDeck>? MabPlayerDecks { get; set; }


        [InverseProperty("MabCampaign")]
        public List<MabPlayerCardCopy>? MabPlayerCardCopies { get; set; }
    }
}
