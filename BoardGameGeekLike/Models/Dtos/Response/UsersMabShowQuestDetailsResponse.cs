namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersMabShowQuestDetailsResponse
    {
        public int? Mab_QuestId { get; set; }

        public string? Mab_QuestTitle { get; set; }

        public string? Mab_QuestDescription { get; set; }


        public List<UsersMabShowQuestDetailsResponse_npc> Mab_Npcs { get; set; }
    }
}
