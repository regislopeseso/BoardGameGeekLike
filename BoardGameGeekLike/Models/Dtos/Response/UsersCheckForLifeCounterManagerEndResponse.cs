namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersCheckForLifeCounterManagerEndResponse
    {
        public bool IsEnded { get; set; } = false;
        public int? Duration_minutes { get; set; }  
    }
}
