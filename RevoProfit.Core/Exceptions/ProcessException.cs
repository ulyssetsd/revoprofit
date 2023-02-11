namespace RevoProfit.Core.Exceptions;

public class ProcessException : Exception
{
    public ProcessException(string? message) : base(message) { }
}