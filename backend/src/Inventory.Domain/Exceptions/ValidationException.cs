namespace Inventory.Domain.Exceptions;

public class ValidationException : DomainException
{
    public override string Code => "VALIDATION_ERROR";
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors) 
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
