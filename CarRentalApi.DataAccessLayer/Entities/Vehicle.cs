using CarRentalApi.DataAccessLayer.Entities.Common;

namespace CarRentalApi.DataAccessLayer.Entities;

public class Vehicle : BaseEntity
{
    public string Brand { get; set; }

    public string Model { get; set; }

    public string Plate { get; set; }

    public string Description { get; set; }

    public DateTime ReleaseDate { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; }
}