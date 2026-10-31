using Claims.Application.Contracts;
using Claims.Application.Services;
using Claims.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController(CoversService covers) : ControllerBase
{
    [HttpPost("compute")]
    public ActionResult<decimal> ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType) =>
        covers.Quote(new CreateCoverRequest { StartDate = startDate, EndDate = endDate, Type = coverType });

    [HttpGet]
    public Task<IReadOnlyList<CoverResponse>> GetAsync(CancellationToken cancellationToken) =>
        covers.ListAsync(cancellationToken);

    [HttpGet("{id}")]
    public Task<CoverResponse?> GetAsync(string id, CancellationToken cancellationToken) =>
        covers.GetAsync(id, cancellationToken);

    [HttpPost]
    public async Task<ActionResult> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken) =>
        Ok(await covers.CreateAsync(request, cancellationToken));

    [HttpDelete("{id}")]
    public Task DeleteAsync(string id, CancellationToken cancellationToken) =>
        covers.DeleteAsync(id, cancellationToken);
}
