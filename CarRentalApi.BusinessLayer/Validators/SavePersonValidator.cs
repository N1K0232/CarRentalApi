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
            .MaximumLength(256)
            .WithMessage("insert a valid first name");

        RuleFor(p => p.LastName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(256)
            .WithMessage("insert a valid last name");

        RuleFor(p => p.DateOfBirth)
            .NotNull()
            .WithMessage("insert a valid date of birth");
    }
}