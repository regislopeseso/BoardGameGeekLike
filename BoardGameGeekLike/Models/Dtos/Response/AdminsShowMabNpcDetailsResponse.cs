namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsShowMabNpcDetailsResponse
    {
        public string? NpcName { get; set; }

        public string? Description { get; set; }

        public int? Level { get; set; }

        public int? DeckSize { get; set; }

        public List<AdminsShowMabNpcDetailsResponse_card>? Cards { get; set; }
    }
}
