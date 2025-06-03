namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersNewLifeCounterRequest
    {
        public string? Name { get; set; }
        public int? StartingLifePoints { get; set; }
        public bool? FixedMaxLife { get; set; }
        public bool? AutoEndMatch { get; set; }
    }
}
