using Claims.Domain;
using Xunit;

namespace Claims.UnitTests;

public class InsurancePeriodTests
{
    [Fact]
    public void Days_do_not_include_the_end_date()
    {
        var period = new InsurancePeriod(new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 31));

        Assert.Equal(30, period.Days);
    }

    [Fact]
    public void A_year_with_29_february_has_366_days()
    {
        var period = new InsurancePeriod(new DateOnly(2028, 1, 1), new DateOnly(2029, 1, 1));

        Assert.Equal(366, period.Days);
    }

    [Fact]
    public void The_period_contains_its_start_date_but_not_its_end_date()
    {
        var period = new InsurancePeriod(new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 31));

        Assert.True(period.Contains(new DateOnly(2027, 1, 1)));
        Assert.True(period.Contains(new DateOnly(2027, 1, 30)));
        Assert.False(period.Contains(new DateOnly(2027, 1, 31)));
        Assert.False(period.Contains(new DateOnly(2026, 12, 31)));
    }
}
