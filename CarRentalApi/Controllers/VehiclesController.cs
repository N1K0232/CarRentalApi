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
	public async Task<IActionResult> Delete(Guid vehicleId)
	{
		var result = await vehicleService.DeleteAsync(vehicleId);
		return CreateResponse(result, StatusCodes.Status200OK);
	}

	[HttpGet("{vehicleId}")]
	public async Task<IActionResult> Get(Guid vehicleId)
	{
		var vehicle = await vehicleService.GetAsync(vehicleId);
		return CreateResponse(vehicle, StatusCodes.Status200OK);
	}

	[HttpGet("{pageIndex}/{itemsPerPage}")]
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
	public async Task<IActionResult> Save([FromBody] SaveVehicleRequest request)
	{
		var result = await vehicleService.SaveAsync(request);
		return CreateResponse(result);
	}
}