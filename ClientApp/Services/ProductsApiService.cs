using System.Net.Http.Json;
using System.Text.Json;
using ClientApp.Models;

namespace ClientApp.Services;

public sealed class ProductsApiService(HttpClient httpClient)
{
    private static readonly TimeSpan ClientCacheDuration = TimeSpan.FromSeconds(15);
    private readonly SemaphoreSlim requestGate = new(1, 1);
    private Product[]? cachedProducts;
    private DateTimeOffset cacheExpiresAt;

    public async Task<Product[]> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        if (cachedProducts is not null && DateTimeOffset.UtcNow < cacheExpiresAt)
        {
            return cachedProducts;
        }

        // AI-assisted refinement (Copilot-style): serialize cache misses so concurrent
        // components reuse one network request instead of fetching the same list twice.
        await requestGate.WaitAsync(cancellationToken);
        try
        {
            if (cachedProducts is not null && DateTimeOffset.UtcNow < cacheExpiresAt)
            {
                return cachedProducts;
            }

            using var response = await httpClient.GetAsync("api/productlist", cancellationToken);
            response.EnsureSuccessStatusCode();

            try
            {
                cachedProducts = await response.Content.ReadFromJsonAsync<Product[]>(cancellationToken: cancellationToken)
                    ?? throw new InvalidDataException("The products API returned an empty response.");
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("The products API returned invalid JSON.", exception);
            }

            cacheExpiresAt = DateTimeOffset.UtcNow.Add(ClientCacheDuration);
            return cachedProducts;
        }
        finally
        {
            requestGate.Release();
        }
    }
}
