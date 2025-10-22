namespace BoardGameGeekLike.Exceptions
{
    public class AuthenticationException : Exception
    {
        public int? RemainingAttempts { get; set; }

        public AuthenticationException(string message, int? remainingAttempts = null) : base(message)
        {
            RemainingAttempts = remainingAttempts;
        }
    }
}
