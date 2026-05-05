namespace LabResource.Domain.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base("You do not have the required permissions to perform this action.")
    {
    }

    public ForbiddenAccessException(string message) : base(message)
    {
    }
}