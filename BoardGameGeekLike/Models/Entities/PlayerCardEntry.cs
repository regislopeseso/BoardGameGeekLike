using System.ComponentModel.DataAnnotations.Schema;

namespace BoardGameGeekLike.Models.Entities
{
    [Table("playerCardEntries")]
    public class PlayerCardEntry
    {
        public int Id { get; set; }

        public bool IsDeleted { get; set; } = false;


        [ForeignKey("MedievalAutoBattlerCampaign")]
        public int MabCampaignId { get; set; }
        [InverseProperty("PlayerCardEntries")]
        public MedievalAutoBattlerCampaign MabCampaign { get; set; }


        [ForeignKey("Card")]
        public int CardId { get; set; }
        public Card Card { get; set; }
    }
}
