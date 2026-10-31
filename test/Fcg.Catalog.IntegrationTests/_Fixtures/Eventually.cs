namespace Fcg.Catalog.IntegrationTests._Fixtures;

public static class Eventually
{
    /// <summary>Reavalia a condicao ate ela ser verdadeira ou o tempo esgotar (processamento assincrono do consumer).</summary>
    public static async Task<bool> TrueAsync(Func<Task<bool>> condition, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(15));

        while (DateTime.UtcNow < deadline)
        {
            if (await condition())
                return true;

            await Task.Delay(200);
        }

        return await condition();
    }
}
