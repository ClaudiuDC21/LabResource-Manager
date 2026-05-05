namespace LabResource.Domain.Exceptions;

public class ConflictException : Exception
{
    public ConflictException() : base("A conflict occurred with the current state of the resource.")
    {
    }

    public ConflictException(string message) : base(message)
    {
    }
}