namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersCheckForLifeCounterManagerEndResponse
    {
        public bool IsFinished { get; set; } = false;
        public int? Duration_minutes { get; set; }  
    }
}
