using CarRentalApi.BusinessLayer.Services;
using CarRentalApi.BusinessLayer.Services.Interfaces;
using CarRentalApi.DataAccessLayer;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OperationResults.AspNetCore;
using System.Text.Json.Serialization;
using TinyHelpers.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();
Configure(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
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

    services.AddMapperProfiles();
    services.AddValidators();

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen()
    .AddFluentValidationRulesToSwagger(options =>
    {
        options.SetNotNullableIfMinLengthGreaterThenZero = true;
    });

    string connectionString = configuration.GetConnectionString("SqlConnection");
    services.AddSqlServer<DataContext>(connectionString);
    services.AddScoped<IDataContext>(services => services.GetRequiredService<DataContext>());

    services.AddScoped<IPeopleService, PeopleService>();
    services.AddScoped<IVehicleService, VehicleService>();
    services.AddScoped<IReservationService, ReservationService>();
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

    app.UseRouting();

    app.UseAuthorization();

    app.UseEndpoints(endpoint =>
    {
        endpoint.MapControllers();
    });
}