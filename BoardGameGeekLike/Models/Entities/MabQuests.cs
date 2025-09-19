using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("MabQuests")]
    public class MabQuest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? Mab_QuestTitle { get; set; }

        public string? Mab_QuestDescription { get; set; }

        public int? Mab_QuestLevel { get; set; }

        public int? Mab_GoldBounty { get; set; }

        public int? Mab_XpReward { get; set; }

        [InverseProperty(nameof(MabNpc.Mab_Quests))]
        public List<MabNpc>? Mab_Npcs { get; set; }

        public bool? Mab_IsDeleted { get; set; } = false;

        [InverseProperty(nameof(MabFulfilledQuest.Mab_Quest))]
        public List<MabFulfilledQuest>? Mab_FulfilledQuests { get; set; }
    }
}
