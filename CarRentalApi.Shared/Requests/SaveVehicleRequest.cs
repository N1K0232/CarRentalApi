using CarRentalApi.Shared.Common;

namespace CarRentalApi.Shared.Requests;

public class SaveVehicleRequest : BaseRequestModel
{
    public string Brand { get; set; }

    public string Model { get; set; }

    public string Plate { get; set; }

    public string Description { get; set; }

    public DateTime? ReleaseDate { get; set; }
}