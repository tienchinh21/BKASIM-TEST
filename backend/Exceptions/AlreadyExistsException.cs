namespace MiniAppGIBA.Exceptions
{
    public class AlreadyExistsException : Exception
    {
        public int _code { get; set; }
        public AlreadyExistsException(string message) : base(message) { }
    }
}
