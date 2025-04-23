using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("categories")]
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]   
        public int Id { get; set; }

        public string? Name { get; set; }

        public List<BoardGame>? BoardGames { get; set; }

        public bool IsDeleted { get; set; } = false;
        public bool IsDummy { get; set; } = false;
    }
}