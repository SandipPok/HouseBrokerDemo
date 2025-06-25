namespace HouseBroker.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
    }

    public class ValidationException : DomainException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("Validation failed") => Errors = errors;
    }

    public class NotFoundException : DomainException
    {
        public NotFoundException(string entity, object id)
            : base($"{entity} with ID {id} not found") { }
    }

    public class UnauthorizedException : DomainException
    {
        public UnauthorizedException() : base("Unauthorized access") { }
    }
}