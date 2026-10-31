using MediatR;

namespace Fcg.Catalog.Application.Games.GetGameById;

public sealed record GetGameByIdQuery(Guid Id) : IRequest<GameResponse>;
