using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarRentalApi.BusinessLayer.Services.Interfaces;
using CarRentalApi.DataAccessLayer;
using CarRentalApi.DataAccessLayer.Entities.Common;
using CarRentalApi.Shared.Common;
using CarRentalApi.Shared.Models;
using CarRentalApi.Shared.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = CarRentalApi.DataAccessLayer.Entities;

namespace CarRentalApi.BusinessLayer.Services;

public class ReservationService : IReservationService
{
	private readonly IDataContext dataContext;
	private readonly IMapper mapper;

	public ReservationService(IDataContext dataContext, IMapper mapper)
	{
		this.dataContext = dataContext;
		this.mapper = mapper;
	}


	public async Task<Result> DeleteAsync(Guid reservationId)
	{
		if (reservationId == Guid.Empty)
		{
			return Result.Fail(FailureReasons.ClientError, "Invalid id");
		}

		var reservation = await dataContext.GetAsync<Entities.Reservation>(reservationId);
		if (reservation != null)
		{
			dataContext.Delete(reservation);
			await dataContext.SaveAsync();
			return Result.Ok();
		}

		return Result.Fail(FailureReasons.ItemNotFound, "No reservation found");
	}

	public async Task<Result<Reservation>> GetAsync(Guid reservationId)
	{
		if (reservationId == Guid.Empty)
		{
			return Result.Fail(FailureReasons.ClientError, "Invalid id");
		}

		var reservation = await dataContext.GetData<Entities.Reservation>()
			.ProjectTo<Reservation>(mapper.ConfigurationProvider)
			.FirstOrDefaultAsync(r => r.Id == reservationId);

		if (reservation != null)
		{
			return reservation;
		}

		return Result.Fail(FailureReasons.ItemNotFound, "No reservation found");
	}

	public async Task<ListResult<Reservation>> GetAsync(int pageIndex, int itemsPerPage)
	{
		var query = dataContext.GetData<Entities.Reservation>();
		var totalCount = await query.CountAsync();
		var reservations = await query.ProjectTo<Reservation>(mapper.ConfigurationProvider)
			.OrderByDescending(r => r.ReservationStart).ThenByDescending(r => r.ReservationEnd)
			.Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1)
			.ToListAsync();

		var result = new ListResult<Reservation>(reservations.Take(itemsPerPage), totalCount, reservations.Count > itemsPerPage);
		return result;
	}

	public async Task<Result<Reservation>> SaveAsync(SaveReservationRequest request)
	{
		var personExist = await ExistsAsync<Entities.Person>(request.PersonId);
		var vehicleExist = await ExistsAsync<Entities.Vehicle>(request.VehicleId);

		if (!personExist)
		{
			return Result.Fail(FailureReasons.ItemNotFound, "Invalid person");
		}

		if (!vehicleExist)
		{
			return Result.Fail(FailureReasons.ItemNotFound, "Invalid vehicle");
		}

		var query = dataContext.GetData<Entities.Reservation>(trackingChanges: true);
		var reservation = request != null ? await query.FirstOrDefaultAsync(r => r.Id == request.Id) : null;

		if (reservation == null)
		{
			reservation = mapper.Map<Entities.Reservation>(request);

			var reservationExist = await dataContext.ExistsAsync<Entities.Reservation>(r => r.PersonId == reservation.PersonId &&
				r.VehicleId == reservation.VehicleId &&
				r.ReservationStart == reservation.ReservationStart &&
				r.ReservationEnd == reservation.ReservationEnd);

			if (reservationExist)
			{
				return Result.Fail(FailureReasons.Conflict);
			}

			dataContext.Insert(reservation);
		}
		else
		{
			mapper.Map(request, reservation);
			dataContext.Edit(reservation);
		}

		await dataContext.SaveAsync();

		var savedReservation = mapper.Map<Reservation>(reservation);
		return savedReservation;
	}

	private Task<bool> ExistsAsync<TEntity>(Guid id) where TEntity : BaseEntity
	{
		return dataContext.ExistsAsync<TEntity>(x => x.Id == id);
	}
}