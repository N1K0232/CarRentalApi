using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace CarRentalApi.Logging;

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration SqlClient(this LoggerSinkConfiguration sinkConfiguration,
        IConfiguration configuration,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        LoggingLevelSwitch levelSwitch = null)
    {
        var sqlSink = new SqlSink(configuration);

        return sinkConfiguration.Sink(sqlSink, restrictedToMinimumLevel, levelSwitch);
    }
}