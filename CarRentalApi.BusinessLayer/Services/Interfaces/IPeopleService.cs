using CarRentalApi.Shared.Common;
using CarRentalApi.Shared.Models;
using CarRentalApi.Shared.Requests;
using OperationResults;

namespace CarRentalApi.BusinessLayer.Services.Interfaces;

public interface IPeopleService
{
    Task<Result> DeleteAsync(Guid personId);

    Task<Result<Person>> GetAsync(Guid personId);

    Task<ListResult<Person>> GetAsync(int pageIndex, int itemsPerPage);

    Task<Result<Person>> SaveAsync(SavePersonRequest request);
}