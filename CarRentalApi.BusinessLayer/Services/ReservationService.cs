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

public class ReservationService : IReservationService
{
	private readonly IDataContext dataContext;
	private readonly IMapper mapper;
	private readonly IValidator<SaveReservationRequest> reservationValidator;
	private readonly ILogger<ReservationService> logger;

	public ReservationService(IDataContext dataContext,
		IMapper mapper,
		IValidator<SaveReservationRequest> reservationValidator,
		ILogger<ReservationService> logger)
	{
		this.dataContext = dataContext;
		this.mapper = mapper;
		this.reservationValidator = reservationValidator;
		this.logger = logger;
	}


	public async Task<Result> DeleteAsync(Guid reservationId)
	{
		logger.LogInformation("deleting reservation");

		if (reservationId.Equals(Guid.Empty))
		{
			logger.LogError("Invalid id", reservationId);
			return Result.Fail(FailureReasons.ClientError, "Invalid id");
		}

		var reservation = await dataContext.GetAsync<Entities.Reservation>(reservationId);
		if (reservation is not null)
		{
			dataContext.Delete(reservation);

			var deletedEntries = await dataContext.SaveAsync();
			if (deletedEntries > 0)
			{
				logger.LogInformation("reservation deleted");
				return Result.Ok();
			}

			logger.LogError("can't delete reservation");
			return Result.Fail(FailureReasons.DatabaseError, "can't delete reservcation");
		}

		logger.LogError("no reservation found");
		return Result.Fail(FailureReasons.ItemNotFound, "No reservation found");
	}

	public async Task<Result<Reservation>> GetAsync(Guid reservationId)
	{
		logger.LogInformation("gets the single reservation");

		if (reservationId.Equals(Guid.Empty))
		{
			return Result.Fail(FailureReasons.ClientError, "Invalid id");
		}

		var reservation = await dataContext.GetData<Entities.Reservation>()
			.ProjectTo<Reservation>(mapper.ConfigurationProvider)
			.FirstOrDefaultAsync(r => r.Id == reservationId);

		if (reservation is not null)
		{
			return reservation;
		}

		return Result.Fail(FailureReasons.ItemNotFound, "No reservation found");
	}

	public async Task<ListResult<Reservation>> GetAsync(int pageIndex, int itemsPerPage)
	{
		logger.LogInformation("gets the list of reservation");

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
		var validationResult = await reservationValidator.ValidateAsync(request);
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

		var personExist = await dataContext.ExistsAsync<Entities.Person>(request.PersonId);
		if (!personExist)
		{
			logger.LogError("Invalid person", request.PersonId);
			return Result.Fail(FailureReasons.ItemNotFound, "Invalid person");
		}

		var vehicleExist = await dataContext.ExistsAsync<Entities.Vehicle>(request.VehicleId);
		if (!vehicleExist)
		{
			logger.LogError("Invalid vehicle", request.VehicleId);
			return Result.Fail(FailureReasons.ItemNotFound, "Invalid vehicle");
		}

		var reservation = request != null ? await dataContext.GetData<Entities.Reservation>(trackingChanges: true)
			.FirstOrDefaultAsync(r => r.Id == request.Id) : null;

		if (reservation is null)
		{
			logger.LogInformation("saving new reservation");

			reservation = mapper.Map<Entities.Reservation>(request);

			var reservationExist = await dataContext.ExistsAsync<Entities.Reservation>(r => r.PersonId == reservation.PersonId &&
				r.VehicleId == reservation.VehicleId &&
				r.ReservationStart == reservation.ReservationStart &&
				r.ReservationEnd == reservation.ReservationEnd);

			if (reservationExist)
			{
				logger.LogError("the reservation already exists");
				return Result.Fail(FailureReasons.Conflict);
			}

			dataContext.Insert(reservation);
		}
		else
		{
			logger.LogError("updating existing reservation");

			mapper.Map(request, reservation);
			dataContext.Edit(reservation);
		}

		var savedEntries = await dataContext.SaveAsync();
		if (savedEntries > 0)
		{
			logger.LogInformation("reservation saved");

			var savedReservation = mapper.Map<Reservation>(reservation);
			return savedReservation;
		}

		logger.LogError("can't save reservation");
		return Result.Fail(FailureReasons.DatabaseError, "can't save reservation");
	}
}