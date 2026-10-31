using MediatR;

namespace Fcg.Catalog.Application.Games.CreateGame;

public sealed record CreateGameCommand(
    string Title,
    string? Description,
    string Genre,
    decimal Price,
    DateOnly? ReleaseDate) : IRequest<GameResponse>;
