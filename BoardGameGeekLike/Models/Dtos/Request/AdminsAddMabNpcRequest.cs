namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class AdminsAddMabNpcRequest
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<int> CardIds { get; set; }
    }
}
