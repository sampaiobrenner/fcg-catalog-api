using System.Text.Json.Nodes;

namespace Fcg.Catalog.IntegrationTests._Fixtures;

/// <summary>Samples de payload copiados do fcg-contracts (fonte de verdade do contrato).</summary>
public static class Samples
{
    public static string Read(string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "_Fixtures", "Samples", fileName));

    public static JsonObject ReadObject(string fileName) => JsonNode.Parse(Read(fileName))!.AsObject();
}
