namespace Claims.Domain;

public static class PremiumCalculator
{
    private const decimal BaseDayRate = 1250m;
    private const int FullRatePeriod = 30;
    private const int FirstDiscountPeriod = 150;

    public static decimal Calculate(CoverType coverType, int days)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(days);

        var dayRate = BaseDayRate * (1 + Surcharge(coverType));
        var (firstDiscount, additionalDiscount) = coverType == CoverType.Yacht ? (0.05m, 0.03m) : (0.02m, 0.01m);

        var fullRateDays = Math.Min(days, FullRatePeriod);
        var firstDiscountDays = Math.Clamp(days - FullRatePeriod, 0, FirstDiscountPeriod);
        var remainingDays = Math.Max(days - FullRatePeriod - FirstDiscountPeriod, 0);

        var premium = dayRate * fullRateDays
            + dayRate * (1 - firstDiscount) * firstDiscountDays
            + dayRate * (1 - firstDiscount - additionalDiscount) * remainingDays;

        return decimal.Round(premium, 2);
    }

    private static decimal Surcharge(CoverType coverType) => coverType switch
    {
        CoverType.Yacht => 0.10m,
        CoverType.PassengerShip => 0.20m,
        CoverType.Tanker => 0.50m,
        CoverType.ContainerShip or CoverType.BulkCarrier => 0.30m,
        _ => throw new ArgumentOutOfRangeException(nameof(coverType), coverType, null)
    };
}
