namespace Inventory.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public override string Code => "ENTITY_NOT_FOUND";

    public EntityNotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
    }
}
