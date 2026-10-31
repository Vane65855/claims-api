using Claims.Application.Contracts;
using Claims.Application.Validation;
using Claims.Domain;
using FluentValidation.TestHelper;
using Xunit;

namespace Claims.UnitTests;

public class CreateClaimRequestValidatorTests
{
    private readonly CreateClaimRequestValidator _validator = new();

    public static IEnumerable<TheoryDataRow<decimal>> AcceptedDamageCosts => [new(0.01m), new(100_000m)];

    public static IEnumerable<TheoryDataRow<decimal>> RejectedDamageCosts => [new(0m), new(-1m), new(100_000.01m)];

    [Theory]
    [MemberData(nameof(AcceptedDamageCosts))]
    public void Damage_cost_up_to_100000_is_accepted(decimal damageCost) =>
        _validator.TestValidate(Request(damageCost: damageCost)).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [MemberData(nameof(RejectedDamageCosts))]
    public void Damage_cost_must_be_positive_and_at_most_100000(decimal damageCost) =>
        _validator.TestValidate(Request(damageCost: damageCost))
            .ShouldHaveValidationErrorFor(request => request.DamageCost);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cover_id_and_name_are_required(string blank)
    {
        var result = _validator.TestValidate(Request(coverId: blank, name: blank));

        result.ShouldHaveValidationErrorFor(request => request.CoverId);
        result.ShouldHaveValidationErrorFor(request => request.Name);
    }

    private static CreateClaimRequest Request(string coverId = "cover-1", string name = "Hull damage", decimal damageCost = 1_000m) =>
        new()
        {
            CoverId = coverId,
            Created = new DateOnly(2026, 11, 1),
            Name = name,
            Type = ClaimType.Collision,
            DamageCost = damageCost
        };
}
