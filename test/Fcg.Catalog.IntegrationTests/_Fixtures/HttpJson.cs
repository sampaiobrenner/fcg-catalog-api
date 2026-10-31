using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fcg.Catalog.IntegrationTests._Fixtures;

public static class HttpJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<T> ReadAsAsync<T>(this HttpResponseMessage response)
        => (await response.Content.ReadFromJsonAsync<T>(Options))!;
}
