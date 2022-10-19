using CarRentalApi.Shared.Common;

namespace CarRentalApi.Shared.Requests;

public class SaveReservationRequest : BaseRequestModel
{
    public Guid PersonId { get; set; }

    public Guid VehicleId { get; set; }

    public DateTime ReservationStart { get; set; }

    public DateTime ReservationEnd { get; set; }
}