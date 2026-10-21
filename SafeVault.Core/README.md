# SafeVault security foundations

This project contains reusable .NET code and NUnit tests for input validation, SQL injection resistance, and XSS output encoding.

## Run the tests

From the `outputs` directory:

```powershell
dotnet test SafeVault.Tests/SafeVault.Tests.csproj
```

## Structure

```text
SafeVault.Core/
├── ParameterizedSql.cs       # Fixed SQL text and separated parameter values
├── SafeHtml.cs               # HTML text-context encoding
├── UserInput.cs              # Validated user data shape
├── UserInputValidator.cs     # Length, format, and allowed-character validation
├── UserRepository.cs         # ADO.NET execution using database parameters
├── UserSql.cs                # Fixed user lookup and insert statements
└── schema.sql                # Example Users table
SafeVault.Tests/
├── UserInputValidatorTests.cs
└── XssProtectionTests.cs
```

Each file has one responsibility. `UserRepository` accepts a provider-specific `DbConnection`, so the hosting application can supply its chosen database provider without embedding provider setup in the security logic.

## Security decisions

- Validation rejects malformed values and bounds their size. It does not attempt to make arbitrary input safe by deleting punctuation.
- SQL commands are fixed strings. User values are passed separately as `DbParameter` values, so a string such as `' OR 1=1 --` remains data and cannot change the query structure.
- `SafeHtml.EncodeText` encodes untrusted text for an HTML text node. In Blazor, normal Razor interpolation already HTML-encodes text; avoid `MarkupString` for untrusted values. Encoding must match the output context (HTML, attribute, URL, JavaScript, etc.).
- The tests verify validation rules, parameter separation, and HTML encoding. They do not replace end-to-end tests against the selected database provider or a security review.

## Application integration

Validate incoming form values with `UserInputValidator.Validate` before calling `UserRepository.AddAsync`. Configure and open the database connection using the provider chosen by the application, then inject it into the repository. For lookups, call `FindByEmailAsync`; do not interpolate user input into SQL. Keep database credentials in a secret store or environment-specific configuration, and do not return raw exception details to clients.

This is the first activity's secure-coding foundation. The repository requires a database provider and an open connection supplied by the application; no database credentials or provider-specific package are included in this sample.

The second activity's runnable authentication and role-authorization API is documented in [SafeVault.Web/README.md](../SafeVault.Web/README.md).
