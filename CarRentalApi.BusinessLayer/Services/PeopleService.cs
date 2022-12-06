using AutoMapper;
using AutoMapper.QueryableExtensions;
using CarRentalApi.BusinessLayer.Services.Interfaces;
using CarRentalApi.DataAccessLayer;
using CarRentalApi.Shared.Common;
using CarRentalApi.Shared.Models;
using CarRentalApi.Shared.Requests;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Entities = CarRentalApi.DataAccessLayer.Entities;

namespace CarRentalApi.BusinessLayer.Services;

public class PeopleService : IPeopleService
{
	private readonly IDataContext dataContext;
	private readonly IMapper mapper;
	private readonly IValidator<SavePersonRequest> personValidator;

	public PeopleService(IDataContext dataContext, IMapper mapper, IValidator<SavePersonRequest> personValidator)
	{
		this.dataContext = dataContext;
		this.mapper = mapper;
		this.personValidator = personValidator;
	}


	public async Task<Result> DeleteAsync(Guid personId)
	{
		if (personId == Guid.Empty)
		{
			return Result.Fail(FailureReasons.ClientError, "Invalid id");
		}

		var person = await dataContext.GetAsync<Entities.Person>(personId);
		if (person != null)
		{
			dataContext.Delete(person);

			var deletedEntries = await dataContext.SaveAsync();
			if (deletedEntries > 0)
			{
				return Result.Ok();
			}

			return Result.Fail(FailureReasons.DatabaseError, "Cannot delete person");
		}

		return Result.Fail(FailureReasons.ItemNotFound, "No person found");
	}

	public async Task<Result<Person>> GetAsync(Guid personId)
	{
		if (personId == Guid.Empty)
		{
			return Result.Fail(FailureReasons.ClientError, "Invalid id");
		}

		var person = await dataContext.GetData<Entities.Person>()
			.ProjectTo<Person>(mapper.ConfigurationProvider)
			.FirstOrDefaultAsync(p => p.Id == personId);

		if (person != null)
		{
			return person;
		}

		return Result.Fail(FailureReasons.ItemNotFound, "No person found");
	}

	public async Task<ListResult<Person>> GetAsync(int pageIndex, int itemsPerPage)
	{
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

			return Result.Fail(FailureReasons.GenericError, validationErrors);
		}

		var person = request.Id != null ? await dataContext.GetData<Entities.Person>(trackingChanges: true)
			.FirstOrDefaultAsync(p => p.Id == request.Id) : null;

		if (person == null)
		{
			person = mapper.Map<Entities.Person>(request);

			var personExist = await dataContext.ExistsAsync<Entities.Person>(p => p.FirstName == person.FirstName &&
				p.LastName == person.LastName &&
				p.DateOfBirth == person.DateOfBirth);

			if (personExist)
			{
				return Result.Fail(FailureReasons.Conflict);
			}

			dataContext.Insert(person);
		}
		else
		{
			mapper.Map(request, person);
			dataContext.Edit(person);
		}

		var savedEntries = await dataContext.SaveAsync();
		if (savedEntries > 0)
		{
			var savedPerson = mapper.Map<Person>(person);
			return savedPerson;
		}

		return Result.Fail(FailureReasons.DatabaseError, "cannot save person");
	}
}