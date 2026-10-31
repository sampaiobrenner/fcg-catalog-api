using MediatR;

namespace Fcg.Catalog.Application.Library.GetMyLibrary;

/// <summary>Biblioteca do usuario autenticado (claim <c>sub</c> do token).</summary>
public sealed record GetMyLibraryQuery : IRequest<IReadOnlyCollection<LibraryItemResponse>>;
