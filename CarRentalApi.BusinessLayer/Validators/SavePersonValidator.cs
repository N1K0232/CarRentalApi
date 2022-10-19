using CarRentalApi.Shared.Requests;
using FluentValidation;

namespace CarRentalApi.BusinessLayer.Validators;

public class SavePersonValidator : AbstractValidator<SavePersonRequest>
{
    public SavePersonValidator()
    {
        RuleFor(p => p.FirstName)
            .NotNull()
            .NotEmpty()
            .WithMessage("the first name is required");

        RuleFor(p => p.LastName)
            .NotNull()
            .NotEmpty()
            .WithMessage("the last name is required");

        RuleFor(p => p.DateOfBirth)
            .NotNull()
            .WithMessage("the date of birth is required");
    }
}