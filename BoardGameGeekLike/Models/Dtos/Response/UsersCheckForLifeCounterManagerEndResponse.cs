namespace BoardGameGeekLike.Models.Dtos.Response
{
    public class UsersCheckForLifeCounterManagerEndResponse
    {
        public bool IsFinished { get; set; } = false;
        public double? Duration_minutes { get; set; }  
    }
}
