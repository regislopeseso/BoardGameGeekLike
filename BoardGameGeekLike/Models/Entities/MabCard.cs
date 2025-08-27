using BoardGameGeekLike.Models.Enums;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("mabCards")]
    public class MabCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Power { get; set; }

        public int UpperHand { get; set; }

        public int Level { get; set; }

        public MabCardType Type { get; set; }

        public bool IsDeleted { get; set; } = false;

        public bool IsDummy { get; set; } = false;


        public List<MabPlayerCardCopy>? MabPlayerCardCopies { get; set; }

        public List<MabNpcCard>? MabNpcCards { get; set; }

    }
}
