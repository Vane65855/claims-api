using Claims.Domain;

namespace Claims.Application.Contracts;

public sealed record CreateClaimRequest
{
    public required string CoverId { get; init; }

    public required DateOnly Created { get; init; }

    public required string Name { get; init; }

    public required ClaimType Type { get; init; }

    public required decimal DamageCost { get; init; }
}

public sealed record ClaimResponse(
    string Id,
    string CoverId,
    DateOnly Created,
    string Name,
    ClaimType Type,
    decimal DamageCost);
