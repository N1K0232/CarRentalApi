using CarRentalApi.BusinessLayer.Services.Interfaces;
using CarRentalApi.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApi.Controllers;

public class ReservationsController : ControllerBase
{
	private readonly IReservationService reservationService;

	public ReservationsController(IReservationService reservationService)
	{
		this.reservationService = reservationService;
	}


	[HttpDelete]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Delete(Guid reservationId)
	{
		var result = await reservationService.DeleteAsync(reservationId);
		return CreateResponse(result, StatusCodes.Status200OK);
	}

	[HttpGet("{reservationId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Get(Guid reservationId)
	{
		var reservation = await reservationService.GetAsync(reservationId);
		return CreateResponse(reservation, StatusCodes.Status200OK);
	}

	[HttpGet("{pageIndex}/{itemsPerPage}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
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
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> Save([FromBody] SaveReservationRequest request)
	{
		var result = await reservationService.SaveAsync(request);
		return CreateResponse(result, StatusCodes.Status201Created);
	}
}