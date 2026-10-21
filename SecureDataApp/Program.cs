using System.Security.Cryptography;
using SecureDataApp.Models;

var admin = new User { Username = "AdminUser", Role = "Admin" };
var basicUser = new User { Username = "BasicUser", Role = "User" };
var encryptionKey = RandomNumberGenerator.GetBytes(32);

using var storage = new SecureStorage(encryptionKey);
CryptographicOperations.ZeroMemory(encryptionKey);

storage.StoreData("Sensitive Information");

var encryptedValue = storage.GetEncryptedDataForDiagnostics();
Console.WriteLine($"Encrypted value at rest (Base64): {encryptedValue}");
if (encryptedValue.Contains("Sensitive Information", StringComparison.Ordinal))
{
    throw new InvalidOperationException("The diagnostic value unexpectedly contains plaintext.");
}

Console.WriteLine("At-rest check passed: the stored value is ciphertext.");

try
{
    Console.WriteLine($"Admin access: {storage.RetrieveData(admin)}");
}
catch (CryptographicException exception)
{
    Console.WriteLine($"Admin decryption failed: {exception.Message}");
}

try
{
    Console.WriteLine($"Basic user access: {storage.RetrieveData(basicUser)}");
}
catch (UnauthorizedAccessException exception)
{
    Console.WriteLine($"Basic user access denied: {exception.Message}");
}
