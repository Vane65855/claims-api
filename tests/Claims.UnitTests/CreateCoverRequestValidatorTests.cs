using Claims.Application.Contracts;
using Claims.Application.Validation;
using Claims.Domain;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Claims.UnitTests;

public class CreateCoverRequestValidatorTests
{
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));
    private readonly CreateCoverRequestValidator _validator;

    public CreateCoverRequestValidatorTests()
    {
        _validator = new CreateCoverRequestValidator(_clock);
    }

    [Fact]
    public void A_cover_starting_today_is_valid() =>
        _validator.TestValidate(Request(new(2026, 10, 30), new(2026, 11, 30))).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void A_cover_starting_yesterday_is_rejected() =>
        _validator.TestValidate(Request(new(2026, 10, 29), new(2026, 11, 30)))
            .ShouldHaveValidationErrorFor(request => request.StartDate);

    [Fact]
    public void Today_comes_from_the_clock_at_validation_time()
    {
        _clock.Advance(TimeSpan.FromDays(1));

        _validator.TestValidate(Request(new(2026, 10, 30), new(2026, 11, 30)))
            .ShouldHaveValidationErrorFor(request => request.StartDate);
    }

    [Fact]
    public void End_date_must_be_after_start_date() =>
        _validator.TestValidate(Request(new(2026, 11, 1), new(2026, 11, 1)))
            .ShouldHaveValidationErrorFor(request => request.EndDate);

    [Fact]
    public void A_full_leap_year_is_valid() =>
        _validator.TestValidate(Request(new(2028, 1, 1), new(2029, 1, 1))).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void A_period_one_day_longer_than_a_year_is_rejected() =>
        _validator.TestValidate(Request(new(2028, 1, 1), new(2029, 1, 2)))
            .ShouldHaveValidationErrorFor(request => request.EndDate);

    [Fact]
    public void A_year_from_29_february_ends_on_28_february()
    {
        _validator.TestValidate(Request(new(2028, 2, 29), new(2029, 2, 28))).ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Request(new(2028, 2, 29), new(2029, 3, 1)))
            .ShouldHaveValidationErrorFor(request => request.EndDate);
    }

    private static CreateCoverRequest Request(DateOnly startDate, DateOnly endDate) =>
        new() { StartDate = startDate, EndDate = endDate, Type = CoverType.Yacht };
}
