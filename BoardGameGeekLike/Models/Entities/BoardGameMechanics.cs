using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("boardgamemechanics")]
    public class BoardGameMechanics
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
        public int Id { get; set; }

        [ForeignKey("BoardGame")]
        public int BoardGameId { get; set; }
        public BoardGame BoardGame { get; set; }


        [ForeignKey("Mechanic")]
        public int MechanicId { get; set; }
        public Mechanic Mechanic { get; set; }


        public bool IsDeleted { get; set; } = false;
    }
}