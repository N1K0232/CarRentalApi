using CarRentalApi.BusinessLayer.Services;
using CarRentalApi.BusinessLayer.Services.Interfaces;
using CarRentalApi.DataAccessLayer;
using CarRentalApi.Documentation;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OperationResults.AspNetCore;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using TinyHelpers.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration, builder.Host);

var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
Configure(app, provider);

await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostBuilder host)
{
    host.UseSerilog((hostingContext, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
    });

    services.AddOperationResult(options =>
    {
        options.ErrorResponseFormat = ErrorResponseFormat.Default;
    });

    services.AddProblemDetails(options =>
    {
        options.Map<OperationCanceledException>(_ => new StatusCodeProblemDetails(StatusCodes.Status408RequestTimeout));
        options.Map<NotImplementedException>(_ => new StatusCodeProblemDetails(StatusCodes.Status503ServiceUnavailable));
        options.Map<DbUpdateException>(_ => new StatusCodeProblemDetails(StatusCodes.Status500InternalServerError));
    });

    services.AddMapperProfiles();
    services.AddValidators();

    services.AddFluentValidationAutoValidation(options =>
    {
        options.DisableDataAnnotationsValidation = true;
    });

    services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
    });

    services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    services.AddEndpointsApiExplorer();

    services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    services.AddSwaggerGen(options =>
    {
        options.OperationFilter<SwaggerDefaultValues>();

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        options.CustomOperationIds(api => $"{api.ActionDescriptor.RouteValues["controller"]}_{api.ActionDescriptor.RouteValues["action"]}");
        options.UseAllOfToExtendReferenceSchemas();
    })
    .AddFluentValidationRulesToSwagger(options =>
    {
        options.SetNotNullableIfMinLengthGreaterThenZero = true;
    });

    var connectionString = configuration.GetConnectionString("SqlConnection");
    services.AddDbContext<IDataContext, DataContext>(options =>
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.CommandTimeout(1);
            sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null);
        });
    });

    services.AddHealthChecks().AddAsyncCheck("sql", async () =>
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message, ex);
        }

        return HealthCheckResult.Healthy();
    });

    services.TryAddScoped<IPeopleService, PeopleService>();
    services.TryAddScoped<IVehicleService, VehicleService>();
    services.TryAddScoped<IReservationService, ReservationService>();
}

void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider provider)
{
    app.UseProblemDetails();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.RoutePrefix = string.Empty;
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
        }
    });

    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging(options =>
    {
        options.IncludeQueryInRequestPath = true;
    });

    app.UseRouting();

    app.UseAuthorization();

    app.UseEndpoints(endpoint =>
    {
        endpoint.MapControllers();

        endpoint.MapHealthChecks("/status", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                var result = JsonSerializer.Serialize(
                            new
                            {
                                Status = report.Status.ToString(),
                                Details = report.Entries.Select(e => new
                                {
                                    Service = e.Key,
                                    Status = Enum.GetName(typeof(HealthStatus), e.Value.Status),
                                    Description = e.Value.Description
                                })
                            });

                context.Response.ContentType = MediaTypeNames.Application.Json;
                await context.Response.WriteAsync(result);
            }
        });
    });
}