# InventoryHub

InventoryHub is a small full-stack inventory sample built from two .NET projects in one solution:

- **ClientApp** is a Blazor WebAssembly app. It loads product data and renders the product list.
- **ServerApp** is a .NET Minimal API. It returns product data as JSON and caches the catalog in memory.

The projects keep one responsibility per file: startup and dependency registration are in `Program.cs`; endpoint mapping, catalog access, API access, UI rendering, and each data model have separate files.

## Contents

- [Requirements](#requirements)
- [Project structure](#project-structure)
- [Run locally](#run-locally)
- [How the integration works](#how-the-integration-works)
- [API contract](#api-contract)
- [Caching and performance](#caching-and-performance)
- [CORS and security](#cors-and-security)
- [Errors and troubleshooting](#errors-and-troubleshooting)
- [Build and verification](#build-and-verification)
- [Known limitations](#known-limitations)
- [Development reflection](#development-reflection)

## Requirements

- .NET 9 SDK
- A terminal and browser
- An HTTPS development certificate only if using the HTTPS launch profiles

Check the installed SDK with:

```powershell
dotnet --version
```

## Project structure

```text
outputs/
├── FullStackSolution.sln
├── README.md
├── REFLECTION.md
├── ClientApp/
│   ├── Program.cs                         # Blazor startup and dependency injection
│   ├── Models/
│   │   ├── Category.cs                    # Client category model
│   │   └── Product.cs                     # Client product model
│   ├── Pages/
│   │   └── FetchProducts.razor             # Product list UI and loading/error states
│   ├── Services/
│   │   └── ProductsApiService.cs           # HTTP, JSON parsing, and client cache
│   └── wwwroot/
│       └── appsettings.json                # API base URL
└── ServerApp/
    ├── Program.cs                          # Server startup, services, and middleware
    ├── Endpoints/
    │   └── ProductEndpoints.cs              # HTTP route mapping
    ├── Models/
    │   ├── Category.cs                      # API category model
    │   └── Product.cs                       # API product model
    └── Services/
        └── ProductCatalogService.cs         # Catalog data and server cache
```

## Run locally

Run the server and client in separate PowerShell terminals from the `outputs` directory.

Terminal 1:

```powershell
cd ServerApp
dotnet run --launch-profile http
```

Terminal 2:

```powershell
cd ClientApp
dotnet run --launch-profile http
```

Open the product page at [http://localhost:5203/fetchproducts](http://localhost:5203/fetchproducts). The API endpoint is [http://localhost:5078/api/productlist](http://localhost:5078/api/productlist).

To stop either application, press `Ctrl+C` in its terminal. If a port is already in use, change the relevant `applicationUrl` in that project's `Properties/launchSettings.json` and update the matching API URL or CORS origin.

### API base URL

The client reads `ProductsApi:BaseUrl` from `ClientApp/wwwroot/appsettings.json`:

```json
{
  "ProductsApi": {
    "BaseUrl": "http://localhost:5078/"
  }
}
```

Keep the trailing slash so the relative request path `api/productlist` resolves as expected. For deployment, set this value to the deployed API's HTTPS origin. Values in `wwwroot/appsettings.json` are public client configuration; do not put secrets there.

## How the integration works

1. Blazor routes `/fetchproducts` to `Pages/FetchProducts.razor`.
2. The page calls the injected `ProductsApiService` once from `OnInitializedAsync`.
3. The service requests `api/productlist` using the configured `HttpClient` base address.
4. `System.Net.Http.Json` deserializes the response into the client `Product` and nested `Category` models.
5. The component renders each product and its category. During a request it displays a loading state; on failure it shows a message and a retry button.

`HttpClient` and `ProductsApiService` are registered in `ClientApp/Program.cs`. The component handles HTTP failures, timeouts, and malformed or empty JSON without leaving the page in a loading state.

## API contract

### `GET /api/productlist`

Returns `200 OK` and an array of products. ASP.NET Core web JSON defaults serialize C# property names as camel case:

```json
[
  {
    "id": 1,
    "name": "Laptop",
    "price": 1200.50,
    "stock": 25,
    "category": {
      "id": 101,
      "name": "Electronics"
    }
  },
  {
    "id": 2,
    "name": "Headphones",
    "price": 50.00,
    "stock": 100,
    "category": {
      "id": 102,
      "name": "Accessories"
    }
  }
]
```

The client and server each define their own models. This keeps the projects independent while preserving the same JSON contract:

- Product: `id` (integer), `name` (string), `price` (number), `stock` (integer), `category` (object)
- Category: `id` (integer), `name` (string)

## Caching and performance

- The client service caches a successful product array for **15 seconds**. A `SemaphoreSlim` ensures concurrent cache misses share one outgoing HTTP request. Failed responses are not cached.
- The server catalog service uses `IMemoryCache` with a **20-second sliding expiration** and a **one-minute absolute expiration**.
- The page does not poll the API. It loads on initialization and only repeats a request when a user selects retry after an error.

The current server catalog is seed data, so server caching demonstrates the approach for a database-backed query. When product create, update, or delete endpoints are introduced, invalidate or refresh the cached catalog after each successful write. The cache intervals are sample defaults and should be tuned against inventory freshness requirements and measured request patterns.

## CORS and security

The API allows the development client origins `http://localhost:5203` and `https://localhost:7270`, configured in `ServerApp/Program.cs`. CORS must include the actual deployed client origin when hosting the application. Avoid an unrestricted origin policy in production.

The sample contains no authentication or authorization and uses in-memory seed data. Add authentication and server-side authorization before exposing business or user-specific inventory. Do not place API secrets in the Blazor WebAssembly project; browser-delivered configuration and code are visible to users. Use HTTPS for deployed traffic.

## Errors and troubleshooting

| Symptom | Check |
| --- | --- |
| Product page cannot reach the API | Start both projects; verify `ProductsApi:BaseUrl` and the server port. |
| Browser reports a CORS error | Confirm the browser's client origin is listed in the server CORS policy. Scheme, host, and port must match. |
| API returns 404 | Confirm the request uses `/api/productlist` and the server endpoint is running. |
| UI reports unexpected response data | Compare the API JSON fields and nesting with the contract above; inspect spelling and property types. |
| Client stays on loading state | Check the browser console and server terminal for request failures. The page should transition to an error state and show a retry button. |
| HTTPS profile does not start | Create or trust a .NET development certificate, or use the provided HTTP profiles for local development. |
| Port binding fails | Check for another process using the port and update launch settings and related URL/CORS configuration consistently. |

## Build and verification

From the `outputs` directory, restore and build the solution:

```powershell
dotnet restore FullStackSolution.sln
dotnet build FullStackSolution.sln
```

Then run both applications using the commands in [Run locally](#run-locally). To verify the data flow, open `/api/productlist` and confirm the response contains `id`, `name`, `price`, `stock`, and nested `category` fields; then open `/fetchproducts` and confirm both products and their categories render. Browser developer tools can confirm request counts, response status, console errors, and request timing.

In the environment where this copy was prepared, both projects compiled successfully in separate builds. Local HTTP requests were blocked and the HTTPS development certificate was unavailable, so an interactive browser check and performance profile could not be collected here. See [REFLECTION.md](REFLECTION.md) for these verification details.

## Known limitations

- Product data is hard-coded seed data; there is no database, inventory editing, or cache invalidation endpoint.
- The client cache and server memory cache are process-local and do not synchronize across multiple server instances.
- Authentication, authorization, logging infrastructure, automated tests, and production deployment configuration are outside this sample's scope.
- Cache durations have not been benchmarked against a production workload.

## Development reflection

The project reflection is in [REFLECTION.md](REFLECTION.md). It describes AI-assisted integration, debugging, JSON modeling, performance changes, challenges, and the verification limits for this environment.
