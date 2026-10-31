namespace Claims.Domain;

public readonly record struct InsurancePeriod(DateOnly Start, DateOnly End)
{
    public int Days => End.DayNumber - Start.DayNumber;
}
