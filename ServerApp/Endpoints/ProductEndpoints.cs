using Microsoft.AspNetCore.Http.HttpResults;
using ServerApp.Models;
using ServerApp.Services;

namespace ServerApp.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/productlist", GetProductListAsync);
        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<Product>>> GetProductListAsync(ProductCatalogService catalog)
    {
        var products = await catalog.GetProductsAsync();
        return TypedResults.Ok(products);
    }
}
