namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class AdminsShowMechanicDetailsResponse
    {
        public int? MechanicId { get; set; }
        public string? MechanicName { get; set; }
        public bool IsDeleted { get; set; }
    }
}
