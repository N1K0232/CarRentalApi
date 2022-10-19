using CarRentalApi.DataAccessLayer.Entities.Common;

namespace CarRentalApi.DataAccessLayer.Entities;

public class Person : BaseEntity
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime DateOfBirth { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; }
}