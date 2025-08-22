using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("mabBattleTurns")]
    public class MabBattleTurn
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? MabBattleRoundNumber { get; set; } = 0;

        public int? MabPlayerCardCopyId { get; set; } = null;

        public int? MabNpcCardCopyId { get; set; } = null;  

        public bool? HasPlayerWon { get; set; }
        
        public bool? IsRoundFinished { get; set; } = false;

        public int? RoundPoints { get; set; } = 0;


        [ForeignKey("MabBattle")]
        public int? MabBattleId { get; set; }
        public MabBattle? MabBattle { get; set; }
    }
}
