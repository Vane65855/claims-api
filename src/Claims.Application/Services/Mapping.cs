using Claims.Application.Contracts;
using Claims.Domain;

namespace Claims.Application.Services;

internal static class Mapping
{
    public static ClaimResponse ToResponse(this Claim claim) =>
        new(claim.Id, claim.CoverId, claim.Created, claim.Name, claim.Type, claim.DamageCost);

    public static CoverResponse ToResponse(this Cover cover) =>
        new(cover.Id, cover.StartDate, cover.EndDate, cover.Type, cover.Premium);
}
