using MediatR;

namespace Fcg.Catalog.Application.Games.DeleteGame;

/// <summary>Desativa o jogo (soft delete), preservando bibliotecas e pedidos.</summary>
public sealed record DeleteGameCommand(Guid Id) : IRequest;
