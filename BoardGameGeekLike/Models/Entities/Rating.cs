using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BoardGameGeekLike.Models.Entities
{
    [Table("ratings")]
    public class Rating

    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int Id { get; set; }

        [Column(TypeName = "decimal(2,1)")]
        public decimal Rate {get; set;}
        

        [ForeignKey("User")]
        public string? UserId {get; set;}
        public User? User {get; set;}


        [ForeignKey("BoardGame")]
        public int BoardGameId {get; set;}
        public BoardGame? BoardGame {get; set;}
    }
}