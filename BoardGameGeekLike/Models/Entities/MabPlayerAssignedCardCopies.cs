using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("MabPlayerAssignedCardCopies")]
    public class MabPlayerAssignedCardCopy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [ForeignKey("MabPlayerCardCopy")]
        public int? MabCardCopyId { get; set; }
        [InverseProperty("MabPlayerAssignedCardCopies")]
        public MabPlayerCardCopy? MabCardCopy { get; set; }


        [ForeignKey("MabPlayerDeck")]
        public int? MabPlayerDeckId { get; set; }
        [InverseProperty("MabPlayerAssignedCardCopies")]
        public MabPlayerDeck? MabPlayerDeck { get; set; }

    }
}
