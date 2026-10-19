using ServerApp.Endpoints;
using ServerApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();
builder.Services.AddCors(options => options.AddPolicy("InventoryHubClient", policy =>
    policy.WithOrigins("http://localhost:5203", "https://localhost:7270")
        .AllowAnyMethod()
        .AllowAnyHeader()));
builder.Services.AddSingleton<ProductCatalogService>();

var app = builder.Build();

app.UseCors("InventoryHubClient");

app.MapProductEndpoints();

app.Run();
