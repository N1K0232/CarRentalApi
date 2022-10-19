using CarRentalApi.Shared.Common;

namespace CarRentalApi.Shared.Models;

public class Person : BaseModel
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime DateOfBirth { get; set; }
}