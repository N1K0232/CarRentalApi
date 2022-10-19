using CarRentalApi.Shared.Common;

namespace CarRentalApi.Shared.Requests;

public class SavePersonRequest : BaseRequestModel
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime? DateOfBirth { get; set; }
}