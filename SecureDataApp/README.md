# SecureDataApp

`SecureDataApp` is a small .NET console application demonstrating encrypted in-memory storage and role-gated retrieval. It stores a sample sensitive string as AES-GCM ciphertext, lets an `Admin` retrieve it, and denies retrieval to a regular user.

## Requirements

- .NET 9 SDK

## Run

From this directory:

```powershell
dotnet run --project SecureDataApp.csproj
```

Or from the parent directory, run the project through the solution:

```powershell
dotnet run --project SecureDataApp/SecureDataApp.csproj
```

The console prints the encrypted representation, confirms the at-rest check, displays the plaintext for the admin, and reports that the regular user was denied.

## Project layout

```text
SecureDataApp/
├── Models/
│   ├── DataStorage.cs   # AES-GCM encryption, in-memory storage, and access check
│   └── User.cs          # Simple user and role model for the demonstration
├── Program.cs           # Creates demo users and runs the example flow
└── README.md            # Project instructions and design notes
```

Each source file has one responsibility: `Program.cs` coordinates the sample, `User.cs` defines the demo user data, and `DataStorage.cs` owns encryption, storage, retrieval, and cleanup.

## Design

- `SecureStorage` encrypts the UTF-8 bytes with **AES-GCM**, which provides confidentiality and detects changes to the ciphertext.
- A fresh cryptographically random 12-byte nonce is generated for each write. The authentication tag is stored with the ciphertext and nonce.
- The encryption key is generated randomly for this run, copied into the storage service, and the original key buffer is cleared. The service clears its key and encrypted buffers when disposed.
- The role check is performed before decryption. Only a user whose role is `Admin` can retrieve the stored value.
- The demonstration outputs a Base64 representation of the nonce, ciphertext, and tag so the encrypted storage can be observed.

## Scope and security notes

This is an educational example, not a production authentication or key-management system. The `User` objects and their roles are created directly in the process, so the role check does not prove a real user's identity. A production application should obtain identity and roles from a trusted authentication system and enforce authorization at the appropriate service or API boundary.

The key exists only in process memory and is discarded when the application exits. The encrypted data is also in memory only, so neither survives a restart. Production key management should use a protected key store, and production data access should avoid exposing diagnostic ciphertext or plaintext in logs and console output. Encryption reduces exposure of stored bytes, but it does not prevent an authorized process or debugger from observing plaintext while it is being used.
