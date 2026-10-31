using Claims.Application.Contracts;
using FluentValidation;

namespace Claims.Application.Validation;

public sealed class CreateClaimRequestValidator : AbstractValidator<CreateClaimRequest>
{
    public CreateClaimRequestValidator()
    {
        RuleFor(request => request.CoverId)
            .NotEmpty()
            .WithMessage("Cover id is required.");

        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Name is required.");

        RuleFor(request => request.DamageCost)
            .GreaterThan(0)
            .WithMessage("Damage cost must be greater than zero.")
            .LessThanOrEqualTo(100_000m)
            .WithMessage("Damage cost cannot exceed 100,000.");
    }
}
