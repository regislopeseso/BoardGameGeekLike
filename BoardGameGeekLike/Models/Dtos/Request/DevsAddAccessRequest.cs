namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class DevsAddAccessRequest
    {
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? BirthDate { get; set; }
        public string? Role { get; set; }
    }
}
