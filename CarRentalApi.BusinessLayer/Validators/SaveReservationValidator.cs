using CarRentalApi.Shared.Requests;
using FluentValidation;

namespace CarRentalApi.BusinessLayer.Validators;

public class SaveReservationValidator : AbstractValidator<SaveReservationRequest>
{
	public SaveReservationValidator()
	{
		RuleFor(r => r.PersonId)
			.NotEmpty()
			.WithMessage("Insert a valid person");

		RuleFor(r => r.VehicleId)
			.NotEmpty()
			.WithMessage("Insert a valid vehicle");

		RuleFor(r => r.ReservationStart)
			.NotNull()
			.WithMessage("Insert a valid date");

		RuleFor(r => r.ReservationEnd)
			.NotNull()
			.WithMessage("Insert a valid date");
	}
}