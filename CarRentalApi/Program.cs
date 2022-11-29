using CarRentalApi.BusinessLayer.Services;
using CarRentalApi.BusinessLayer.Services.Interfaces;
using CarRentalApi.DataAccessLayer;
using Hellang.Middleware.ProblemDetails;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
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
    services.AddOperationResult();
    services.AddProblemDetails();
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
    app.UseAuthorization();
    app.UseRouting();
    app.UseEndpoints(endpoint =>
    {
        endpoint.MapControllers();
    });
}