using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("boardgames")]
    public class BoardGame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string Description { get; set; } = String.Empty;

        public required int MinPlayersCount { get; set; }

        public required int MaxPlayersCount { get; set; }

        public required int MinAge { get; set; }


        [ForeignKey("Category")]
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public List<Mechanic>? Mechanics { get; set; }

        public int AverageRating {get; set;}

        public int RatingsCount {get; set;}

        [InverseProperty("BoardGame")]
        public List<Session>? Sessions {get; set;}

        public bool IsDeleted { get; set; } = false;
    }
}