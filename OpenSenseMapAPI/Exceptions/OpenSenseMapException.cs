namespace OpenSenseMapAPI.Exceptions;

public class OpenSenseMapException : Exception
{
    public int StatusCode { get; set; }

    public OpenSenseMapException(string message, int statusCode = 500) : base(message)
    {
        StatusCode = statusCode;
    }

    public OpenSenseMapException(string message, Exception innerException, int statusCode = 500) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
