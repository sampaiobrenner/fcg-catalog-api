using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application._Shared.ReadQueries;
using Fcg.Catalog.Application._Shared.Security;
using Fcg.Catalog.Application.Properties;
using MediatR;

namespace Fcg.Catalog.Application.Library.GetMyLibrary;

internal sealed class GetMyLibraryQueryHandler : IRequestHandler<GetMyLibraryQuery, IReadOnlyCollection<LibraryItemResponse>>
{
    private readonly IGameReadQueries _readQueries;
    private readonly ICurrentUser _currentUser;

    public GetMyLibraryQueryHandler(IGameReadQueries readQueries, ICurrentUser currentUser)
    {
        _readQueries = readQueries;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyCollection<LibraryItemResponse>> Handle(GetMyLibraryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException(ApplicationResources.UsuarioNaoAutenticado);
        return _readQueries.GetUserLibraryAsync(userId, cancellationToken);
    }
}
