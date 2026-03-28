using System.Net;
using System.Runtime.Serialization;

namespace Bloggi.Backend.Api.Web.Exceptions;

public class BloggiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    
    public Dictionary<string, string> Errors { get; } = new();
    
    public BloggiException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
    
    public BloggiException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
    {
        StatusCode = statusCode;
    }

}