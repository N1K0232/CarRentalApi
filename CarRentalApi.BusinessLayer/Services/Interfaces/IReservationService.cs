using CarRentalApi.Shared.Common;
using CarRentalApi.Shared.Models;
using CarRentalApi.Shared.Requests;
using OperationResults;

namespace CarRentalApi.BusinessLayer.Services.Interfaces;
public interface IReservationService
{
    Task<Result> DeleteAsync(Guid reservationId);
    Task<Result<Reservation>> GetAsync(Guid reservationId);
    Task<ListResult<Reservation>> GetAsync(int pageIndex, int itemsPerPage);
    Task<Result<Reservation>> SaveAsync(SaveReservationRequest request);
}