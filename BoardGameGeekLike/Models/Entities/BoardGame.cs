using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("boardgames")]
    public class BoardGame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public int MinPlayersCount { get; set; }

        public int MaxPlayersCount { get; set; }

        public int MinAge { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}