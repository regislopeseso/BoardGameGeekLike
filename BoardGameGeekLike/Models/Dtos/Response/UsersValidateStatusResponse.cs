namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersValidateStatusResponse
    {
        public bool IsUserLoggedIn { get; set; } = false;

        public string? Role { get; set; } = null;
    }
}
