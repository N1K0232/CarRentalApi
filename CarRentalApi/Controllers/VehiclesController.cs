using CarRentalApi.BusinessLayer.Services.Interfaces;
using CarRentalApi.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApi.Controllers;

public class VehiclesController : ControllerBase
{
	private readonly IVehicleService vehicleService;

	public VehiclesController(IVehicleService vehicleService)
	{
		this.vehicleService = vehicleService;
	}


	[HttpDelete]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Delete(Guid vehicleId)
	{
		var result = await vehicleService.DeleteAsync(vehicleId);
		return CreateResponse(result, StatusCodes.Status200OK);
	}

	[HttpGet("{vehicleId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Get(Guid vehicleId)
	{
		var vehicle = await vehicleService.GetAsync(vehicleId);
		return CreateResponse(vehicle, StatusCodes.Status200OK);
	}

	[HttpGet("{pageIndex}/{itemsPerPage}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Get(int pageIndex = 0, int itemsPerPage = 10)
	{
		var vehicles = await vehicleService.GetAsync(pageIndex, itemsPerPage);
		if (vehicles.Content.Any())
		{
			return Ok(vehicles);
		}

		return NotFound("No vehicle found");
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> Save([FromBody] SaveVehicleRequest request)
	{
		var result = await vehicleService.SaveAsync(request);
		return CreateResponse(result, StatusCodes.Status201Created);
	}
}