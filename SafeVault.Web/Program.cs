using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using SafeVault.Core.Authentication;
using SafeVault.Web;

var builder = WebApplication.CreateBuilder(args);

var keyRingPath = builder.Configuration["SAFEVAULT_DATA_PROTECTION_KEY_PATH"]
    ?? Path.Combine(builder.Environment.ContentRootPath, ".keys");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
    .SetApplicationName("SafeVault.Web");
builder.Services.AddSafeVaultAuthentication();
builder.Services.Configure<PasswordHasherOptions>(options => options.IterationCount = 210_000);
builder.Services.AddSingleton<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();
builder.Services.AddSingleton(serviceProvider => DemoAccountStoreFactory.Create(
    builder.Configuration,
    serviceProvider.GetRequiredService<IPasswordHasher<UserAccount>>()));
builder.Services.AddSingleton<IUserAccountStore>(serviceProvider =>
    serviceProvider.GetRequiredService<InMemoryUserAccountStore>());
builder.Services.AddSingleton<CredentialAuthenticator>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapAuthenticationEndpoints();

app.Run();
