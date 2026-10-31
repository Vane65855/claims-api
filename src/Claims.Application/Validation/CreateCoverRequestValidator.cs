using Claims.Application.Contracts;
using FluentValidation;

namespace Claims.Application.Validation;

public sealed class CreateCoverRequestValidator : AbstractValidator<CreateCoverRequest>
{
    public CreateCoverRequestValidator(TimeProvider clock)
    {
        RuleFor(request => request.StartDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime))
            .WithMessage("Start date cannot be in the past.");

        RuleFor(request => request.EndDate)
            .GreaterThan(request => request.StartDate)
            .WithMessage("End date must be after the start date.")
            .LessThanOrEqualTo(request => request.StartDate.AddYears(1))
            .WithMessage("The insurance period cannot be longer than one year.");
    }
}
