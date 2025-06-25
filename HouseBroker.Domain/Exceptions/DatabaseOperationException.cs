namespace HouseBroker.Domain.Exceptions
{
    public class DatabaseOperationException : Exception
    {
        public string OperationType { get; }
        public DatabaseOperationException(
            string operationType,
            string message,
            Exception inner) : base(message, inner)
        {
            OperationType = operationType;
        }
    }
}
