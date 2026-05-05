namespace LabResource.VerticalApi.Common.Exceptions;

public class AlreadyExistsException : Exception
{
    public AlreadyExistsException() : base("The resource already exists in the system.")
    {
    }

    public AlreadyExistsException(string message) : base(message)
    {
    }

    public AlreadyExistsException(string name, object key)
        : base($"Entity \"{name}\" with key ({key}) already exists in the system.")
    {
    }
}