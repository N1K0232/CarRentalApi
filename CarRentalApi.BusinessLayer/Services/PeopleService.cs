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

public class PeopleService : IPeopleService
{
	private readonly IDataContext dataContext;
	private readonly IMapper mapper;
	private readonly IValidator<SavePersonRequest> personValidator;
	private readonly ILogger<PeopleService> logger;

	public PeopleService(IDataContext dataContext,
		IMapper mapper,
		IValidator<SavePersonRequest> personValidator,
		ILogger<PeopleService> logger)
	{
		this.dataContext = dataContext;
		this.mapper = mapper;
		this.personValidator = personValidator;
		this.logger = logger;
	}


	public async Task<Result> DeleteAsync(Guid personId)
	{
		logger.LogInformation("delete person");

		if (personId.Equals(Guid.Empty))
		{
			logger.LogError("Invalid id", personId);
			return Result.Fail(FailureReasons.ClientError, "Invalid id");
		}

		var person = await dataContext.GetAsync<Entities.Person>(personId);
		if (person is not null)
		{
			dataContext.Delete(person);

			var deletedEntries = await dataContext.SaveAsync();
			if (deletedEntries > 0)
			{
				logger.LogInformation("person deleted");
				return Result.Ok();
			}

			logger.LogError("Can't delete person", person);
			return Result.Fail(FailureReasons.DatabaseError, "Can't delete person");
		}

		logger.LogError("No person found");
		return Result.Fail(FailureReasons.ItemNotFound, "No person found");
	}

	public async Task<Result<Person>> GetAsync(Guid personId)
	{
		logger.LogError("get the single person");

		if (personId.Equals(Guid.Empty))
		{
			logger.LogError("Invalid id", personId);
			return Result.Fail(FailureReasons.ClientError, "Invalid id");
		}

		var person = await dataContext.GetData<Entities.Person>()
			.ProjectTo<Person>(mapper.ConfigurationProvider)
			.FirstOrDefaultAsync(p => p.Id == personId);

		if (person is not null)
		{
			return person;
		}

		logger.LogError("No person found", person);
		return Result.Fail(FailureReasons.ItemNotFound, "No person found");
	}

	public async Task<ListResult<Person>> GetAsync(int pageIndex, int itemsPerPage)
	{
		logger.LogInformation("gets the list of people");

		var query = dataContext.GetData<Entities.Person>();
		var totalCount = await query.CountAsync();

		var people = await query.ProjectTo<Person>(mapper.ConfigurationProvider)
			.OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
			.Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1)
			.ToListAsync();

		var result = new ListResult<Person>(people.Take(itemsPerPage), totalCount, people.Count > itemsPerPage);
		return result;
	}

	public async Task<Result<Person>> SaveAsync(SavePersonRequest request)
	{
		var validationResult = await personValidator.ValidateAsync(request);
		if (!validationResult.IsValid)
		{
			var validationErrors = new List<ValidationError>();

			foreach (var error in validationResult.Errors.DistinctBy(e => e.PropertyName))
			{
				validationErrors.Add(new ValidationError(error.PropertyName, error.ErrorMessage));
			}

			logger.LogError("invalid request", validationErrors);
			return Result.Fail(FailureReasons.GenericError, validationErrors);
		}

		var person = request.Id != null ? await dataContext.GetData<Entities.Person>(trackingChanges: true)
			.FirstOrDefaultAsync(p => p.Id == request.Id) : null;

		if (person is null)
		{
			logger.LogInformation("saving new person");

			person = mapper.Map<Entities.Person>(request);

			var personExist = await dataContext.ExistsAsync<Entities.Person>(p => p.FirstName == person.FirstName &&
				p.LastName == person.LastName &&
				p.DateOfBirth == person.DateOfBirth);

			if (personExist)
			{
				logger.LogError("the person already exists", person);
				return Result.Fail(FailureReasons.Conflict);
			}

			dataContext.Insert(person);
		}
		else
		{
			logger.LogInformation("updating existing person");

			mapper.Map(request, person);
			dataContext.Edit(person);
		}

		var savedEntries = await dataContext.SaveAsync();
		if (savedEntries > 0)
		{
			logger.LogInformation("saved person");

			var savedPerson = mapper.Map<Person>(person);
			return savedPerson;
		}

		logger.LogError("can't save person");
		return Result.Fail(FailureReasons.DatabaseError, "can't save person");
	}
}