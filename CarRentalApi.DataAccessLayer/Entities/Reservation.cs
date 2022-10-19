using CarRentalApi.DataAccessLayer.Entities.Common;

namespace CarRentalApi.DataAccessLayer.Entities;

public class Reservation : DeletableEntity
{
    public Guid PersonId { get; set; }

    public Guid VehicleId { get; set; }

    public DateTime ReservationStart { get; set; }

    public DateTime ReservationEnd { get; set; }

    public virtual Person Person { get; set; }

    public virtual Vehicle Vehicle { get; set; }
}