using CarRentalApi.Shared.Common;

namespace CarRentalApi.Shared.Models;

public class Vehicle : BaseModel
{
    public string Brand { get; set; }

    public string Model { get; set; }

    public string Plate { get; set; }

    public string Description { get; set; }

    public DateTime ReleaseDate { get; set; }
}