namespace Claims.Domain;

public readonly record struct InsurancePeriod(DateOnly Start, DateOnly End)
{
    public int Days => End.DayNumber - Start.DayNumber;

    public bool Contains(DateOnly date) => Start <= date && date < End;
}
