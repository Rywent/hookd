namespace Hookd.Application.Exceptions;

public class ConflictException : BusinessException
{
    public string ResourceType { get; }
    public string ConflictField { get; }

    public ConflictException(string resourceType, string field, string? customMessage = null,
        Exception? innerException = null)
        : base(customMessage ?? $"{resourceType} with {field} already exists.", innerException)
    {
        ResourceType = resourceType;
        ConflictField = field;
    }
}
