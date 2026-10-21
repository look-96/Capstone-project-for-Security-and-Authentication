using System.Data.Common;

namespace SafeVault.Core;

/// <summary>Executes fixed SQL statements with values supplied as database parameters.</summary>
public sealed class UserRepository(DbConnection connection)
{
    public async Task AddAsync(UserInput user, CancellationToken cancellationToken = default)
    {
        var statement = UserSql.Insert(user);
        await using var command = CreateCommand(statement);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<UserInput?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var statement = UserSql.FindByEmail(email);
        await using var command = CreateCommand(statement);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? new UserInput(reader.GetString(1), reader.GetString(2))
            : null;
    }

    private DbCommand CreateCommand(ParameterizedSql statement)
    {
        var command = connection.CreateCommand();
        command.CommandText = statement.CommandText;

        foreach (var (name, value) in statement.Parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        return command;
    }
}
