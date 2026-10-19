using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ClientApp;
using ClientApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var productsApiBaseUrl = builder.Configuration["ProductsApi:BaseUrl"]
    ?? throw new InvalidOperationException("ProductsApi:BaseUrl must be configured.");

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(productsApiBaseUrl),
    Timeout = TimeSpan.FromSeconds(10)
});
builder.Services.AddScoped<ProductsApiService>();

await builder.Build().RunAsync();
