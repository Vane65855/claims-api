using Claims.Domain;
using Xunit;

namespace Claims.UnitTests;

public class PremiumCalculatorTests
{
    public static IEnumerable<TheoryDataRow<int, decimal>> YachtPremiums =>
    [
        new(0, 0m),
        new(1, 1_375.00m),
        new(29, 39_875.00m),
        new(30, 41_250.00m),
        new(31, 42_556.25m),
        new(45, 60_843.75m),
        new(179, 235_881.25m),
        new(180, 237_187.50m),
        new(181, 238_452.50m),
        new(364, 469_947.50m),
        new(365, 471_212.50m),
        new(366, 472_477.50m)
    ];

    public static IEnumerable<TheoryDataRow<CoverType, int, decimal>> PremiumsByType =>
    [
        new(CoverType.Yacht, 1, 1_375.00m),
        new(CoverType.PassengerShip, 1, 1_500.00m),
        new(CoverType.ContainerShip, 1, 1_625.00m),
        new(CoverType.BulkCarrier, 1, 1_625.00m),
        new(CoverType.Tanker, 1, 1_875.00m),
        new(CoverType.BulkCarrier, 31, 50_342.50m),
        new(CoverType.ContainerShip, 200, 319_150.00m),
        new(CoverType.Tanker, 181, 333_693.75m),
        new(CoverType.Tanker, 365, 668_343.75m),
        new(CoverType.Tanker, 366, 670_162.50m)
    ];

    [Theory]
    [MemberData(nameof(YachtPremiums))]
    public void Yacht_premium_follows_the_day_rate_bands(int days, decimal expected) =>
        Assert.Equal(expected, PremiumCalculator.Calculate(CoverType.Yacht, days));

    [Theory]
    [MemberData(nameof(PremiumsByType))]
    public void Premium_depends_on_cover_type_and_days(CoverType type, int days, decimal expected) =>
        Assert.Equal(expected, PremiumCalculator.Calculate(type, days));

    [Fact]
    public void Negative_days_throw() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => PremiumCalculator.Calculate(CoverType.Yacht, -1));

    [Fact]
    public void An_undefined_cover_type_throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => PremiumCalculator.Calculate((CoverType)42, 10));
}
