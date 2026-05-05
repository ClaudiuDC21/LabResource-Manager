namespace LabResource.Domain.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException() : base("The request is invalid or malformed.")
    {
    }

    public BadRequestException(string message) : base(message)
    {
    }
}