using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("ratings")]
    public class Rating

    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int Id { get; set; }
        public int Rate {get; set;}
        

        [ForeignKey("User")]
        public int UserId {get; set;}
        public User? User {get; set;}


        [ForeignKey("BoardGame")]
        public int BoardGameId {get; set;}
        public BoardGame? BoardGame {get; set;}
    }
}