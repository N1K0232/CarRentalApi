using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarRentalApi.BusinessLayer.Services.Interfaces;
using CarRentalApi.DataAccessLayer;
using CarRentalApi.Shared.Common;
using CarRentalApi.Shared.Models;
using CarRentalApi.Shared.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = CarRentalApi.DataAccessLayer.Entities;

namespace CarRentalApi.BusinessLayer.Services;

public class VehicleService : IVehicleService
{
	private readonly IDataContext dataContext;
	private readonly IMapper mapper;

	public VehicleService(IDataContext dataContext, IMapper mapper)
	{
		this.dataContext = dataContext;
		this.mapper = mapper;
	}


	public async Task<Result> DeleteAsync(Guid vehicleId)
	{
		if (vehicleId == Guid.Empty)
		{
			return Result.Fail(FailureReasons.ClientError, "Invalid id");
		}

		var vehicle = await dataContext.GetAsync<Entities.Vehicle>(vehicleId);
		if (vehicle != null)
		{
			dataContext.Delete(vehicle);
			await dataContext.SaveAsync();
			return Result.Ok();
		}

		return Result.Fail(FailureReasons.ItemNotFound, "No vehicle found");
	}

	public async Task<Result<Vehicle>> GetAsync(Guid vehicleId)
	{
		if (vehicleId == Guid.Empty)
		{
			return Result.Fail(FailureReasons.ClientError, "Invalid id");
		}

		var vehicle = await dataContext.GetData<Entities.Vehicle>()
			.ProjectTo<Vehicle>(mapper.ConfigurationProvider)
			.FirstOrDefaultAsync(v => v.Id == vehicleId);

		if (vehicle != null)
		{
			return vehicle;
		}

		return Result.Fail(FailureReasons.ItemNotFound, "No vehicle found");
	}

	public async Task<ListResult<Vehicle>> GetAsync(int pageIndex, int itemsPerPage)
	{
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
		var query = dataContext.GetData<Entities.Vehicle>(trackingChanges: true);
		var vehicle = request.Id != null ? await query.FirstOrDefaultAsync(v => v.Id == request.Id) : null;

		if (vehicle == null)
		{
			vehicle = mapper.Map<Entities.Vehicle>(request);

			var exists = await dataContext.ExistsAsync<Entities.Vehicle>(v => v.Brand == vehicle.Brand &&
				v.Model == vehicle.Model &&
				v.Plate == vehicle.Plate &&
				v.Description == vehicle.Description &&
				v.ReleaseDate == vehicle.ReleaseDate);

			if (exists)
			{
				return Result.Fail(FailureReasons.Conflict);
			}

			dataContext.Insert(vehicle);
		}
		else
		{
			mapper.Map(request, vehicle);
			dataContext.Edit(vehicle);
		}

		await dataContext.SaveAsync();

		var savedVehicle = mapper.Map<Vehicle>(vehicle);
		return savedVehicle;
	}
}