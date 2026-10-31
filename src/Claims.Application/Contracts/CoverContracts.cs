using Claims.Domain;

namespace Claims.Application.Contracts;

public sealed record CreateCoverRequest
{
    public required DateOnly StartDate { get; init; }

    public required DateOnly EndDate { get; init; }

    public required CoverType Type { get; init; }
}

public sealed record CoverResponse(
    string Id,
    DateOnly StartDate,
    DateOnly EndDate,
    CoverType Type,
    decimal Premium);
