using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Events;
using System.Data;

namespace CarRentalApi.Logging;

public class SqlSink : ILogEventSink
{
    private SqlConnection connection;
    private readonly IConfiguration configuration;

    public SqlSink(IConfiguration configuration)
    {
        connection = null;
        this.configuration = configuration;
    }

    private string ConnectionString => configuration.GetConnectionString("SqlConnection");


    public void Emit(LogEvent logEvent)
    {
        var commandText = "INSERT INTO Logs (Message,Level,TimeStamp,Exception) " +
                $"VALUES(@Message,@Level,@TimeStamp,@Exception)";

        string connectionString = ConnectionString;
        connection = new SqlConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = commandText;

        CreateParameter(command, "@Message", DbType.String, logEvent.RenderMessage());
        CreateParameter(command, "@Level", DbType.String, logEvent.Level.ToString());
        CreateParameter(command, "@TimeStamp", DbType.DateTime, logEvent.Timestamp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        CreateParameter(command, "@exception", DbType.String, logEvent.Exception?.ToString());

        command.ExecuteNonQuery();

        connection.Close();
        connection.Dispose();
    }

    private static void CreateParameter(IDbCommand command, string name, DbType type, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;
        parameter.Value = value ?? DBNull.Value;

        command.Parameters.Add(parameter);
    }
}