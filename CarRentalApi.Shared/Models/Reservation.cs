using CarRentalApi.Shared.Common;

namespace CarRentalApi.Shared.Models;

public class Reservation : BaseModel
{
    public Person Person { get; set; }

    public Vehicle Vehicle { get; set; }

    public DateTime ReservationStart { get; set; }

    public DateTime ReservationEnd { get; set; }
}