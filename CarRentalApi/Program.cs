using CarRentalApi.BusinessLayer.Services;
using CarRentalApi.BusinessLayer.Services.Interfaces;
using CarRentalApi.DataAccessLayer;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OperationResults.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;
using TinyHelpers.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration, builder.Host);

var app = builder.Build();
Configure(app);

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

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen()
    .AddFluentValidationRulesToSwagger(options =>
    {
        options.SetNotNullableIfMinLengthGreaterThenZero = true;
    });

    services.AddDbContext<IDataContext, DataContext>(options =>
    {
        var connectionString = configuration.GetConnectionString("SqlConnection");
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.CommandTimeout(1);
            sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null);
        });
    });

    services.TryAddScoped<IPeopleService, PeopleService>();
    services.TryAddScoped<IVehicleService, VehicleService>();
    services.TryAddScoped<IReservationService, ReservationService>();
}

void Configure(IApplicationBuilder app)
{
    app.UseProblemDetails();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Car Rental Api v1");
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
    });
}