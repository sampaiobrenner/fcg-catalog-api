using Fcg.Catalog.Application.Properties;

namespace Fcg.Catalog.Application._Shared.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string resource, object key)
        : base(string.Format(ApplicationResources.RecursoNaoEncontrado, resource, key))
    {
    }
}
