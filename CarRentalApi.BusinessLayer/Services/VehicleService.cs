using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarRentalApi.BusinessLayer.Services.Interfaces;
using CarRentalApi.DataAccessLayer;
using CarRentalApi.Shared.Common;
using CarRentalApi.Shared.Models;
using CarRentalApi.Shared.Requests;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OperationResults;
using Entities = CarRentalApi.DataAccessLayer.Entities;

namespace CarRentalApi.BusinessLayer.Services;

public class VehicleService : IVehicleService
{
	private readonly IDataContext dataContext;
	private readonly IMapper mapper;
	private readonly IValidator<SaveVehicleRequest> vehicleValidator;
	private readonly ILogger<VehicleService> logger;

	public VehicleService(IDataContext dataContext,
		IMapper mapper,
		IValidator<SaveVehicleRequest> vehicleValidator,
		ILogger<VehicleService> logger)
	{
		this.dataContext = dataContext;
		this.mapper = mapper;
		this.vehicleValidator = vehicleValidator;
		this.logger = logger;
	}


	public async Task<Result> DeleteAsync(Guid vehicleId)
	{
		logger.LogInformation("deleting vehicle");

		if (vehicleId.Equals(Guid.Empty))
		{
			logger.LogError("Invalid id", vehicleId);
			return Result.Fail(FailureReasons.ClientError, "Invalid id");
		}

		var vehicle = await dataContext.GetAsync<Entities.Vehicle>(vehicleId);
		if (vehicle is not null)
		{
			dataContext.Delete(vehicle);

			var deletedEntries = await dataContext.SaveAsync();
			if (deletedEntries > 0)
			{
				logger.LogInformation("vehicle deleted");
				return Result.Ok();
			}

			logger.LogError("can't delete vehicle");
			return Result.Fail(FailureReasons.DatabaseError, "Can't delete vehicle");
		}

		logger.LogError("No vehicle found");
		return Result.Fail(FailureReasons.ItemNotFound, "No vehicle found");
	}

	public async Task<Result<Vehicle>> GetAsync(Guid vehicleId)
	{
		logger.LogInformation("gets the single vehicle");

		if (vehicleId.Equals(Guid.Empty))
		{
			logger.LogError("Invalid id", vehicleId);
			return Result.Fail(FailureReasons.ClientError, "Invalid id");
		}

		var vehicle = await dataContext.GetData<Entities.Vehicle>()
			.ProjectTo<Vehicle>(mapper.ConfigurationProvider)
			.FirstOrDefaultAsync(v => v.Id == vehicleId);

		if (vehicle is not null)
		{
			return vehicle;
		}

		logger.LogError("No vehicle found");
		return Result.Fail(FailureReasons.ItemNotFound, "No vehicle found");
	}

	public async Task<ListResult<Vehicle>> GetAsync(int pageIndex, int itemsPerPage)
	{
		logger.LogInformation("gets the list of vehicles");

		var query = dataContext.GetData<Entities.Vehicle>();
		var totalCount = await query.CountAsync();

		var vehicles = await query.ProjectTo<Vehicle>(mapper.ConfigurationProvider)
			.OrderBy(v => v.Brand).ThenBy(v => v.Model)
			.Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1)
			.ToListAsync();

		var result = new ListResult<Vehicle>(vehicles.Take(itemsPerPage), totalCount, vehicles.Count > itemsPerPage);
		return result;
	}

	public async Task<Result<Vehicle>> SaveAsync(SaveVehicleRequest request)
	{
		var validationResult = await vehicleValidator.ValidateAsync(request);
		if (!validationResult.IsValid)
		{
			var validationErrors = new List<ValidationError>();

			foreach (var error in validationResult.Errors.DistinctBy(e => e.PropertyName))
			{
				validationErrors.Add(new ValidationError(error.PropertyName, error.ErrorMessage));
			}

			logger.LogError("Invalid request", validationErrors);
			return Result.Fail(FailureReasons.GenericError, validationErrors);
		}

		var vehicle = request.Id != null ? await dataContext.GetData<Entities.Vehicle>(trackingChanges: true).FirstOrDefaultAsync(v => v.Id == request.Id) : null;

		if (vehicle is null)
		{
			logger.LogInformation("saving new vehicle");

			vehicle = mapper.Map<Entities.Vehicle>(request);

			var vehicleExist = await dataContext.ExistsAsync<Entities.Vehicle>(v => v.Brand == vehicle.Brand &&
				v.Model == vehicle.Model &&
				v.Plate == vehicle.Plate &&
				v.Description == vehicle.Description &&
				v.ReleaseDate == vehicle.ReleaseDate);

			if (vehicleExist)
			{
				logger.LogError("the vehicle already exists");
				return Result.Fail(FailureReasons.Conflict);
			}

			dataContext.Insert(vehicle);
		}
		else
		{
			logger.LogInformation("updating existing vehicle");

			mapper.Map(request, vehicle);
			dataContext.Edit(vehicle);
		}

		var savedEntries = await dataContext.SaveAsync();
		if (savedEntries > 0)
		{
			logger.LogInformation("saved vehicle");

			var savedVehicle = mapper.Map<Vehicle>(vehicle);
			return savedVehicle;
		}

		logger.LogError("can't save vehicle");
		return Result.Fail(FailureReasons.DatabaseError, "can't save vehicle");
	}
}