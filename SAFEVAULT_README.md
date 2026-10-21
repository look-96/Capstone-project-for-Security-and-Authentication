# SafeVault: Secure Coding and Authentication

SafeVault is a teaching solution that consolidates the work from the three security activities: secure input and database access, authentication and role authorization, and review of SQL injection and XSS protections. The project is organized around one responsibility per file and keeps shared security logic separate from HTTP hosting.

## Solution contents

- `SafeVault.Core` — input validation, parameterized SQL construction and execution, output encoding, and authentication domain logic.
- `SafeVault.Web` — ASP.NET Core API, dependency injection, cookie authentication, login throttling, and protected endpoints.
- `SafeVault.Tests` — NUnit tests for validation, SQL injection resistance, XSS encoding, and authentication/role behavior.
- `SAFEVAULT_SECURITY_SUMMARY.md` — findings, verification record, and development-tool disclosure.

Open `SafeVault.sln` to work on SafeVault alone. `FullStackSolution.sln` remains the umbrella solution for the other projects in this workspace.

## Requirements

- .NET 9 SDK
- An HTTPS development certificate for the default local launch profile. Create/trust one if needed with `dotnet dev-certs https --trust`.

## Build and run

From this directory in PowerShell:

```powershell
dotnet build SafeVault.sln
$env:SAFEVAULT_ADMIN_PASSWORD = "Choose-a-unique-admin-password-123!"
$env:SAFEVAULT_USER_PASSWORD = "Choose-a-unique-user-password-123!"
dotnet run --project SafeVault.Web/SafeVault.Web.csproj
```

The sample requires both environment variables and rejects passwords shorter than 12 characters. Optional username variables are `SAFEVAULT_ADMIN_USERNAME` and `SAFEVAULT_USER_USERNAME`; defaults are `admin` and `user`. Use disposable local credentials only. Never commit real credentials.

Test the suite with:

```powershell
dotnet test SafeVault.sln
```

The tests are compiled with the solution. In the development environment used to prepare this project, the NUnit test host stalled before executing tests; see `SAFEVAULT_SECURITY_SUMMARY.md` for the exact verification status. Run the command in a normal .NET environment and review the result before treating the suite as passed.

## Activity 1 — secure input and data access

`UserInputValidator` checks username and email shape and length. Validation improves data quality but is not the primary SQL injection defense. SQL statements are fixed in `UserSql`; `ParameterizedSql` and `UserRepository` bind user values as database parameters instead of concatenating values into SQL text. This keeps values such as `' OR 1=1 --` as data. `SafeHtml.EncodeText` uses the platform HTML encoder for text output. In Blazor, render untrusted values as ordinary Razor text and do not pass them to `MarkupString` unless they have been safely sanitized for the exact HTML context.

## Activity 2 — authentication and authorization

The API verifies submitted passwords using ASP.NET Core Identity's `PasswordHasher<TUser>` and creates a claims principal after successful verification. Passwords are hashed, not stored in plaintext or reversibly encrypted. Cookie middleware protects the authenticated principal. The login endpoint has a per-client-IP rate limit, and the admin dashboard requires the `admin` role. The in-memory account store is a demo implementation, not persistent user management.

Available routes:

- `POST /auth/login` — accepts `{ "username": "admin", "password": "..." }` and sets an authentication cookie on success.
- `POST /auth/logout` — signs out the current user.
- `GET /api/profile` — requires an authenticated user.
- `GET /api/admin/dashboard` — requires the `admin` role; a signed-in regular user is denied.

The cookie is HttpOnly, Secure, SameSite Strict, uses a `__Host-` name, and expires after 30 minutes. Invalid credentials receive the same unauthorized response. The login limiter allows five attempts per minute per client IP.

## Activity 3 — vulnerability review and fixes

The review checks for SQL string concatenation and unsafe HTML rendering. The data-access code uses parameters, while the output helper encodes text. Tests include SQL injection strings and script, image-event, and SVG-event XSS payloads. Since the reviewed API returns JSON and has no HTML rendering sink for user-generated content, the XSS protection is verified at the encoding boundary rather than by claiming that input filtering alone prevents XSS. See `SAFEVAULT_SECURITY_SUMMARY.md` for what was reviewed and which checks could and could not run in this environment.

## Security and deployment boundaries

This is a learning sample, not a production-ready identity system. Replace the in-memory store with persistent account storage, add account-level lockout and audit controls, and review the rate-limit strategy for multi-instance deployment. Keep Data Protection keys in a protected, shared key store in production; the local `.keys` directory is development-only and ignored by Git. Keep HTTPS enabled. If browser clients are cross-origin and use cookies, configure a narrow CORS policy and anti-forgery protections rather than allowing every origin.

## File responsibility

Each source file has a focused role: validators validate, SQL helpers define/bind statements, repositories perform persistence operations, authentication services verify credentials, endpoint files map HTTP routes, and tests verify behavior. This makes the security boundaries easier to review and individual pieces easier to replace.

## Summarizing Copilot assistance

I used Copilot for input validation, database access, authentication, and output handling. The review checked for SQL statements built by concatenating user input and for untrusted content rendered as raw HTML. SafeVault already used parameterized SQL, and its text-encoding helper encoded potentially unsafe markup, so I kept those protections and expanded the security tests with SQL injection and XSS payloads. I also reviewed the authentication and role checks.
The Core, Web, and Tests projects compiled successfully.
