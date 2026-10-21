# SafeVault authentication and authorization

This activity adds a runnable ASP.NET Core API to the SafeVault security foundation. It verifies passwords using ASP.NET Core Identity's `PasswordHasher<TUser>`, issues a protected authentication cookie, and restricts the admin dashboard to accounts with the `admin` role.

## Requirements and start-up

- .NET 9 SDK
- An HTTPS development certificate (`dotnet dev-certs https --trust` if one is not already trusted)

Set two demo passwords in the current PowerShell session and start the API from the `outputs` directory:

```powershell
$env:SAFEVAULT_ADMIN_PASSWORD = "Use-a-unique-admin-password-123!"
$env:SAFEVAULT_USER_PASSWORD = "Use-a-unique-user-password-123!"
dotnet run --project SafeVault.Web/SafeVault.Web.csproj
```

The demo usernames default to `admin` and `user`; optionally set `SAFEVAULT_ADMIN_USERNAME` and `SAFEVAULT_USER_USERNAME`. Both passwords are required and must be at least 12 characters. The app fails during startup if they are missing or too short. Do not use the sample strings as real credentials.

## Endpoints

- `POST /auth/login` — JSON body `{ "username": "admin", "password": "..." }`; successful login sets the authentication cookie.
- `POST /auth/logout` — signs out the authenticated user.
- `GET /api/profile` — requires an authenticated user and returns the username and role.
- `GET /api/admin/dashboard` — requires the `admin` role; a normal `user` receives HTTP 403.

The login endpoint is limited to five attempts per minute per client IP. Invalid usernames and passwords both return HTTP 401. The cookie is HttpOnly, Secure, SameSite Strict, uses a `__Host-` name, and expires after 30 minutes. The middleware returns 401/403 status codes rather than redirecting API clients to an HTML login page.

For local development, the cookie Data Protection key ring is kept under `SafeVault.Web/.keys/`, which is excluded from source control. This local key ring is not protected by a production key-encryption mechanism. Configure `SAFEVAULT_DATA_PROTECTION_KEY_PATH` for an appropriate protected, shared key store before production deployment.

## Authentication and role flow

`DemoAccountStoreFactory` reads demo credentials from environment configuration and hashes them at startup. `CredentialAuthenticator` looks up the account and verifies the password hash; unknown usernames are checked against a dummy hash so the response path is less useful for account enumeration. Successful authentication creates a `ClaimsPrincipal` with the account name and role. ASP.NET Core cookie middleware protects that principal between requests. The admin route applies a role authorization policy before its handler runs.

ASP.NET Core Identity's password hasher stores salted, versioned PBKDF2 hashes; passwords are never stored as plain text or encrypted for later recovery. The iteration count is configured to 210,000 for this sample.

## File layout

```text
SafeVault.Core/Authentication/
├── CredentialAuthenticator.cs     # Credential verification and claims creation
├── IUserAccountStore.cs            # Account lookup boundary
├── InMemoryUserAccountStore.cs     # Demo-only account lookup implementation
└── UserAccount.cs                  # Account and password-hash model
SafeVault.Web/
├── AuthenticationEndpoints.cs     # Login, logout, profile, and admin routes
├── AuthenticationSetup.cs         # Cookie, authorization, and login rate limit
├── DemoAccountStoreFactory.cs      # Demo account bootstrap from environment values
├── LoginRequest.cs                 # Login request shape
└── Program.cs                      # Dependency injection and middleware pipeline
SafeVault.Tests/
└── CredentialAuthenticatorTests.cs # Valid login, invalid login, and role tests
```

Each file has one responsibility. `SafeVault.Core` contains the authentication logic and storage boundary; `SafeVault.Web` composes the HTTP application and routes; `SafeVault.Tests` holds the NUnit test cases.

## Tests

From `outputs`:

```powershell
dotnet test SafeVault.Tests/SafeVault.Tests.csproj
```

The authentication tests cover a correct password, an incorrect password, an unknown username, and the role claims that control admin access. The earlier input-validation, SQL parameterization, and XSS tests remain in the same test project.

## Production boundary

This is a teaching implementation. `InMemoryUserAccountStore` is deliberately replaceable through `IUserAccountStore`; it does not persist users or lock accounts after failed attempts. A production implementation should store password hashes and roles in a persistent database, apply per-account lockout and audit controls, and manage data-protection keys in a protected shared store when running multiple instances. Keep the API and Blazor UI same-origin where possible. If using cross-origin cookie authentication, configure a narrow CORS policy and anti-forgery protection rather than relaxing SameSite or CORS broadly.
