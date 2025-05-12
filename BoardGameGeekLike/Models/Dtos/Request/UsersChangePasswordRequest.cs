namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersChangePasswordRequest
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
