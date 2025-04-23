using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("mechanics")]
    public class Mechanic
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
        public int Id { get; set; }

        public required string Name { get; set; }

        public List<BoardGame>? BoardGames {get; set;}

        public bool IsDeleted { get; set; } = false;

        public bool IsDummy { get; set; } = false;
    }
}