namespace Fcg.Catalog.Application._Shared.Parsers;

public interface IParser<TInput, TOutput>
{
    TOutput? Parse(TInput? source);

    IReadOnlyCollection<TOutput> Parse(IEnumerable<TInput>? source);
}
