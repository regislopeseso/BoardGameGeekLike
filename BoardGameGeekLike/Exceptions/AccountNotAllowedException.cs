namespace BoardGameGeekLike.Exceptions
{
    public class AccountNotAllowedException : Exception
    {
        public AccountNotAllowedException(string message) : base(message) { }
    }
}
