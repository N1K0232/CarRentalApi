using CarRentalApi.BusinessLayer.Services.Interfaces;
using CarRentalApi.Shared.Requests;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApi.Controllers;

public class ReservationsController : ControllerBase
{
	private readonly IReservationService reservationService;
	private readonly IValidator<SaveReservationRequest> reservationValidator;

	public ReservationsController(IReservationService reservationService, IValidator<SaveReservationRequest> reservationValidator)
	{
		this.reservationService = reservationService;
		this.reservationValidator = reservationValidator;
	}


	[HttpDelete]
	public async Task<IActionResult> Delete(Guid reservationId)
	{
		var result = await reservationService.DeleteAsync(reservationId);
		return CreateResponse(result, StatusCodes.Status200OK);
	}

	[HttpGet("{reservationId}")]
	public async Task<IActionResult> Get(Guid reservationId)
	{
		var reservation = await reservationService.GetAsync(reservationId);
		return CreateResponse(reservation, StatusCodes.Status200OK);
	}

	[HttpGet("{pageIndex}/{itemsPerPage}")]
	public async Task<IActionResult> Get(int pageIndex = 0, int itemsPerPage = 10)
	{
		var reservations = await reservationService.GetAsync(pageIndex, itemsPerPage);
		if (reservations.Content.Any())
		{
			return Ok(reservations);
		}

		return NotFound("No reservation found");
	}

	[HttpPost]
	public async Task<IActionResult> Save([FromBody] SaveReservationRequest request)
	{
		var validationResult = reservationValidator.Validate(request);
		if (validationResult.IsValid)
		{
			var result = await reservationService.SaveAsync(request);
			return CreateResponse(result, StatusCodes.Status200OK);
		}

		return BadRequest(validationResult);
	}
}