namespace MiniAppGIBA.Exceptions
{
    public class CustomException : Exception
    {
        public int Code { get; set; }
        public new object? Data { get; set; }

        public CustomException(int code, string message, object? data = null) : base(message)
        {
            Code = code;
            Data = data;
        }

        public CustomException(string message) : base(message)
        {
            Code = 400;
        }

        public CustomException(string message, Exception innerException) : base(message, innerException)
        {
            Code = 500;
        }
    }
}
