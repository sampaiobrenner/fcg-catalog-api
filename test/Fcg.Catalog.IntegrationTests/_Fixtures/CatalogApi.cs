using System.Net;
using System.Net.Http.Json;
using Fcg.Catalog.Application.Games;
using Fcg.Catalog.Application.Promotions;

namespace Fcg.Catalog.IntegrationTests._Fixtures;

/// <summary>Atalhos para preparar dados pela propria API (como administrador).</summary>
public static class CatalogApi
{
    public static async Task<GameResponse> CreateGameAsync(this HttpClient admin, decimal price, string? genre = null, string? title = null)
    {
        var response = await admin.PostAsJsonAsync("/api/v1/games", new
        {
            title = title ?? $"Jogo {Guid.CreateVersion7():N}",
            description = "Jogo de teste",
            genre = genre ?? $"Genero-{Guid.CreateVersion7():N}",
            price,
            releaseDate = "2024-05-10"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.ReadAsAsync<GameResponse>();
    }

    public static async Task<PromotionResponse> CreatePromotionAsync(this HttpClient admin, decimal discount, params Guid[] gameIds)
    {
        var response = await admin.PostAsJsonAsync("/api/v1/promotions", new
        {
            name = $"Promo {discount}%",
            discountPercent = discount,
            startsAt = DateTime.UtcNow.AddDays(-1),
            endsAt = DateTime.UtcNow.AddDays(1),
            gameIds
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.ReadAsAsync<PromotionResponse>();
    }
}
