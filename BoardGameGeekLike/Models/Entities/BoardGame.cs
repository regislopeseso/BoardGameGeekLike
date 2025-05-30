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
      
        [Column(TypeName = "decimal(2,1)")]
        public decimal AverageRating {get; set;}

        public int RatingsCount {get; set;}

        public int AvgDuration_minutes { get; set; }

        [InverseProperty("BoardGame")]
        public List<Session>? Sessions {get; set;}

        public int SessionsCount { get; set; }

        public bool IsDeleted { get; set; } = false;
        public bool IsDummy { get; set; } = false;
    }
}