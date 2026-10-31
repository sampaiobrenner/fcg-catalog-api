using Fcg.Catalog.Application._Shared.Exceptions;
using Fcg.Catalog.Application._Shared.ReadQueries;
using Fcg.Catalog.Application._Shared.Security;
using Fcg.Catalog.Application.Library.GetMyLibrary;
using Moq;

namespace Fcg.Catalog.UnitTests.Application.Library.GetMyLibrary;

public class GetMyLibraryQueryHandlerTests
{
    private readonly Mock<IGameReadQueries> _readQueries = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    [Fact]
    public async Task Handle_DeveConsultarBibliotecaDoUsuarioDoToken()
    {
        var userId = Guid.CreateVersion7();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _readQueries.Setup(r => r.GetUserLibraryAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new LibraryItemResponse { GameId = Guid.CreateVersion7(), Title = "Hades" }]);

        var result = await new GetMyLibraryQueryHandler(_readQueries.Object, _currentUser.Object).Handle(new GetMyLibraryQuery(), CancellationToken.None);

        result.Should().ContainSingle().Which.Title.Should().Be("Hades");
    }

    [Fact]
    public async Task Handle_DeveLancarNaoAutenticado_QuandoTokenSemSub()
    {
        _currentUser.Setup(u => u.UserId).Returns((Guid?)null);

        var act = () => new GetMyLibraryQueryHandler(_readQueries.Object, _currentUser.Object).Handle(new GetMyLibraryQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
