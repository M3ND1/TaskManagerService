namespace TaskManager.Core.Exceptions;

public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException() { }
    public ConcurrencyConflictException(string message) : base(message) { }
    public ConcurrencyConflictException(string message, Exception innerException) : base(message, innerException) { }
}
