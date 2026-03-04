using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace WSIST.Engine;
using MySql.Data.MySqlClient;

public class Database
{
    public readonly MySqlConnection Connection;

    public Database(IOptions<DatabaseOption> options)
    {
        Connection = new MySqlConnection(options.Value.ConnectionString);
        Connection.Open();
    }

    public DataTable Query(string sqlQuery, Dictionary<string, object>? parameters = null)
    {
        using var command = new MySqlCommand(
            sqlQuery,
            Connection
        );

        if (parameters != null){
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue('@' + param.Key, param.Value);
            }
        }

        using var reader = command.ExecuteReader();

        var table = new DataTable();
        table.Load(reader);

        return table;
    }
}

public record DatabaseOption
{
    public string ConnectionString { get; set; } = null!;
}