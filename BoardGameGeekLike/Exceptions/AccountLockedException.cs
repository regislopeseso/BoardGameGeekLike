namespace BoardGameGeekLike.Exceptions
{
    public class AccountLockedException : Exception
    {
        public AccountLockedException(string message) : base(message) { }
    }
}
