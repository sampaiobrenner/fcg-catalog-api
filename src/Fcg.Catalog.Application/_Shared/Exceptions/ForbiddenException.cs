namespace Fcg.Catalog.Application._Shared.Exceptions;

public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message)
        : base(message)
    {
    }
}
