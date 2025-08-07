using BoardGameGeekLike.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("mabCampains")]
    public class MedievalAutoBattlerCampain
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public required string Name { get; set; }

        public int PlayerLevel { get; set; }

        public int Gold { get; set; }

        public int CountMatches { get; set; }

        public int CountVictories { get; set; }

        public int CountDefeats { get; set; }

        public int CountBoosters { get; set; }

        public bool AllCardsCollectedTrophy { get; set; } = false;

        public bool AllNpcsDefeatedTrophy { get; set; } = false;

        public bool IsDeleted { get; set; } = false;


        [ForeignKey("User")]
        public string? UserId { get; set; }
        public User? User { get; set; }


        [InverseProperty("PlayerCardEntries")]
        public List<PlayerCardEntry> PlayerCardEntries { get; set; }


        [InverseProperty("Decks")]
        public List<Deck> Decks { get; set; }        
    }
}
