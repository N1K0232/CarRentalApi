using CarRentalApi.Shared.Requests;
using FluentValidation;

namespace CarRentalApi.BusinessLayer.Validators;

public class SaveVehicleValidator : AbstractValidator<SaveVehicleRequest>
{
	public SaveVehicleValidator()
	{
		RuleFor(v => v.Brand)
			.NotNull()
			.NotEmpty()
			.MaximumLength(100)
			.WithMessage("insert a valid brand");

		RuleFor(v => v.Model)
			.NotNull()
			.NotEmpty()
			.MaximumLength(100)
			.WithMessage("insert a valid model");

		RuleFor(v => v.Plate)
			.NotNull()
			.NotEmpty()
			.MaximumLength(100)
			.WithMessage("insert a valid plate");

		RuleFor(v => v.ReleaseDate)
			.NotNull()
			.WithMessage("insert a valid release date");
	}
}