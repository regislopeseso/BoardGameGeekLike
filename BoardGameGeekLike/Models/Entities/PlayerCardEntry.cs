using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("playerCardEntries")]
    public class PlayerCardEntry
    {
        public int Id { get; set; }

        public bool IsDeleted { get; set; } = false;



        [ForeignKey("Save")]
        public int SaveId { get; set; }
        [InverseProperty("PlayerCardEntries")]
        public PlayerSave Save { get; set; }


        [ForeignKey("Card")]
        public int CardId { get; set; }
        public Card Card { get; set; }
    }
}
