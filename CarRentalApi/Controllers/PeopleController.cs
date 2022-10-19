using CarRentalApi.BusinessLayer.Services.Interfaces;
using CarRentalApi.Shared.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApi.Controllers;

public class PeopleController : ControllerBase
{
	private readonly IPeopleService peopleService;

	public PeopleController(IPeopleService peopleService)
	{
		this.peopleService = peopleService;
	}


	[HttpDelete]
	public async Task<IActionResult> Delete(Guid personId)
	{
		var result = await peopleService.DeleteAsync(personId);
		return CreateResponse(result, StatusCodes.Status200OK);
	}

	[HttpGet("{personId}")]
	public async Task<IActionResult> Get(Guid personId)
	{
		var person = await peopleService.GetAsync(personId);
		return CreateResponse(person, StatusCodes.Status200OK);
	}

	[HttpGet("{pageIndex}/{itemsPerPage}")]
	public async Task<IActionResult> Get(int pageIndex = 0, int itemsPerPage = 10)
	{
		var people = await peopleService.GetAsync(pageIndex, itemsPerPage);
		if (people.Content.Any())
		{
			return Ok(people);
		}

		return NotFound("No person found");
	}

	[HttpPost]
	public async Task<IActionResult> Save([FromBody] SavePersonRequest request)
	{
		var result = await peopleService.SaveAsync(request);
		return CreateResponse(result, StatusCodes.Status200OK);
	}
}