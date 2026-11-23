using System.Data.Common;
using System.Globalization;

namespace Architecture_1.Infrastructure;

public abstract class RelationalDatabase : IDisposable
{
    private readonly DbConnection _connection;

    protected RelationalDatabase(DbConnection connection, string path, string schema)
    {
        _connection = connection;
        try
        {
            _connection.Open();
            using var transaction = _connection.BeginTransaction();
            using var command = _connection.CreateCommand();
            command.CommandText = schema;
            command.Transaction = transaction;
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (DbException error)
        {
            _connection.Dispose();
            throw new InvalidOperationException($"Не удалось открыть базу «{path}»: {error.Message}", error);
        }
    }

    internal IReadOnlyList<T> Query<T>(string sql, Func<DbDataReader, T> read,
        params (string Name, object? Value)[] parameters)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value switch
            {
                null => DBNull.Value,
                decimal amount => amount.ToString(CultureInfo.InvariantCulture),
                Guid id => id.ToString(),
                _ => value
            };
            command.Parameters.Add(parameter);
        }

        try
        {
            using var reader = command.ExecuteReader();
            var rows = new List<T>();
            while (reader.Read())
                rows.Add(read(reader));

            return rows;
        }
        catch (DbException error)
        {
            throw new InvalidOperationException($"Не удалось выполнить запрос: {error.Message}", error);
        }
    }

    internal static decimal ReadDecimal(DbDataReader row, int index) =>
        decimal.Parse(row.GetString(index), CultureInfo.InvariantCulture);

    public void Dispose() => _connection.Dispose();
}
