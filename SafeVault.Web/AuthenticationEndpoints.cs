using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SafeVault.Core.Authentication;

namespace SafeVault.Web;

public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/login", LoginAsync)
            .AllowAnonymous()
            .RequireRateLimiting("login");

        app.MapPost("/auth/logout", (Delegate)LogoutAsync)
            .RequireAuthorization();

        app.MapGet("/api/profile", (ClaimsPrincipal user) => Results.Ok(new
        {
            Username = user.Identity?.Name,
            Roles = user.FindAll(ClaimTypes.Role).Select(claim => claim.Value)
        }))
        .RequireAuthorization();

        app.MapGet("/api/admin/dashboard", () => Results.Ok(new
        {
            Message = "Admin-only dashboard data."
        }))
        .RequireAuthorization(policy => policy.RequireRole("admin"));
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest? request,
        CredentialAuthenticator authenticator,
        HttpContext httpContext)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return Results.BadRequest(new { Error = "Username and password are required." });
        }

        var principal = await authenticator.AuthenticateAsync(request.Username, request.Password);
        if (principal is null)
        {
            return Results.Unauthorized();
        }

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return Results.NoContent();
    }

    private static async Task<IResult> LogoutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.NoContent();
    }
}
