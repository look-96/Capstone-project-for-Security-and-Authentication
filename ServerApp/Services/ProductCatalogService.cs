using Microsoft.Extensions.Caching.Memory;
using ServerApp.Models;

namespace ServerApp.Services;

public sealed class ProductCatalogService(IMemoryCache cache)
{
    private const string CacheKey = "inventoryhub:productlist:v1";

    public Task<IReadOnlyList<Product>> GetProductsAsync() =>
        cache.GetOrCreateAsync(CacheKey, entry =>
        {
            // Replace the seed data with a repository query when persistence is added.
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            entry.SlidingExpiration = TimeSpan.FromSeconds(20);
            return Task.FromResult<IReadOnlyList<Product>>
            ([
                new Product(1, "Laptop", 1200.50m, 25, new Category(101, "Electronics")),
                new Product(2, "Headphones", 50.00m, 100, new Category(102, "Accessories"))
            ]);
        })!;
}
