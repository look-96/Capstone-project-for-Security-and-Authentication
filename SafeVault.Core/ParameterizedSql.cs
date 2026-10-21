namespace SafeVault.Core;

public sealed record ParameterizedSql(string CommandText, IReadOnlyDictionary<string, object> Parameters);
