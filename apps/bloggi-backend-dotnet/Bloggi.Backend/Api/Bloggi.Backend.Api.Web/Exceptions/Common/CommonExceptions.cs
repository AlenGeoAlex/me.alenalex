using System.Net;

namespace Bloggi.Backend.Api.Web.Exceptions.Common;

public class FailedToIdentifyUserException() : BloggiException(HttpStatusCode.Unauthorized, "Failed to identify user.");