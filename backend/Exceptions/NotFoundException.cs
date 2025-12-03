namespace MiniAppGIBA.Exceptions
{
    public class NotFoundException : Exception
    {
        public int Code { get; set; }
        public NotFoundException() { }

        public NotFoundException(string message) : base(message) { }

        public NotFoundException(int code, string message) : base(message)
        {
            Code = code;
        }
    }
}
