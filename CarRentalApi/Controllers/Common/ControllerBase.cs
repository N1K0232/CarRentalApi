using Microsoft.AspNetCore.Mvc;
using OperationResults;
using OperationResults.AspNetCore;
using System.Net.Mime;

namespace CarRentalApi.Controllers.Common;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public abstract class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
{
	protected ControllerBase()
	{
	}

	protected IActionResult CreateResponse(Result result, int? statusCode = null)
	{
		return HttpContext.CreateResponse(result, statusCode);
	}

	protected IActionResult CreateResponse<T>(Result<T> result, int? statusCode = null)
	{
		return HttpContext.CreateResponse(result, statusCode);
	}
}