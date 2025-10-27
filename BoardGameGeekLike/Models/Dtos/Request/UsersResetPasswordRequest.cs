namespace BoardGameGeekLike.Models.Dtos.Request
{
    public class UsersResetPasswordRequest
    {
        public string? UserEmail { get; set; } = string.Empty;

        public string? NewPassword { get; set; } = string.Empty;

        public string? Token { get; set; } = string.Empty;
    }
}
