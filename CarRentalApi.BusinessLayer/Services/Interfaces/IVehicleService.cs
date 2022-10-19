using CarRentalApi.Shared.Common;
using CarRentalApi.Shared.Models;
using CarRentalApi.Shared.Requests;
using OperationResults;

namespace CarRentalApi.BusinessLayer.Services.Interfaces;
public interface IVehicleService
{
    Task<Result> DeleteAsync(Guid vehicleId);
    Task<Result<Vehicle>> GetAsync(Guid vehicleId);
    Task<ListResult<Vehicle>> GetAsync(int pageIndex, int itemsPerPage);
    Task<Result<Vehicle>> SaveAsync(SaveVehicleRequest request);
}