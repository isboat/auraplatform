namespace Aura.Api.Common;

public sealed class ServiceException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
