using MediatR;

namespace Fcg.Catalog.Application.Games.UpdateGame;

public sealed record UpdateGameCommand(
    Guid Id,
    string Title,
    string? Description,
    string Genre,
    decimal Price,
    DateOnly? ReleaseDate) : IRequest<GameResponse>;
