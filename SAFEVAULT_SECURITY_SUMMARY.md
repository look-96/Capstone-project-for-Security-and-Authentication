# SafeVault Security Review Summary

## Findings and fixes

- **SQL injection:** The review found no query built by concatenating user input. `UserSql` keeps SQL text fixed, and `UserRepository` binds values through `DbParameter`. The attack tests now cover both `' OR 1=1 --` and `'; DROP TABLE Users;--`, asserting the payload remains a parameter rather than entering the SQL text.
- **XSS:** SafeVault has an HTML text encoder in `SafeHtml.EncodeText`, and the Blazor guidance in the Core README warns against rendering untrusted content through `MarkupString`. Tests now cover script, image `onerror`, and SVG `onload` payloads and verify their markup is encoded. The API currently returns JSON and has no HTML rendering sink for user-generated content.
- **Input validation:** `UserInputValidator` enforces username character/length rules and email format/length limits. Attack tests cover script-like usernames and malformed email values. Validation is an additional boundary; SQL parameters and output-context encoding provide the primary SQLi/XSS defenses.

No insecure SQL concatenation or raw HTML rendering path was present in the reviewed SafeVault code, so this activity primarily confirmed the existing protections and expanded the attack cases rather than replacing an unsafe implementation.

## Verification

- `dotnet build SafeVault.Web/SafeVault.Web.csproj --no-restore` succeeded with no warnings.
- `dotnet build SafeVault.Tests/SafeVault.Tests.csproj --no-restore` succeeded. Restore emitted `NU1900` because the NuGet vulnerability feed was unreachable.
- Direct security smoke checks passed for the SQL injection payloads, XSS payloads, and hostile input validation. Authentication smoke checks from Activity 2 also passed.
- The NUnit test host stalled before executing tests in this environment. HTTP route checks were blocked by local socket restrictions and the absence of an HTTPS development certificate. The NUnit cases are included and compile, but should be run with `dotnet test SafeVault.Tests/SafeVault.Tests.csproj` in a normal .NET development environment.

## Copilot disclosure

Microsoft Copilot was not used in this Codex session. OpenAI Codex assisted with reviewing the SafeVault code, strengthening attack-focused tests, and documenting the findings. This disclosure is included so the submission does not claim Copilot use that did not occur.
