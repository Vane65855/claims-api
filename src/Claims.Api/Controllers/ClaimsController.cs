using Claims.Application.Contracts;
using Claims.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ClaimsController(ClaimsService claims) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ClaimResponse>> GetAsync(CancellationToken cancellationToken) =>
        claims.ListAsync(cancellationToken);

    [HttpPost]
    public async Task<ActionResult> CreateAsync(CreateClaimRequest request, CancellationToken cancellationToken) =>
        Ok(await claims.CreateAsync(request, cancellationToken));

    [HttpDelete("{id}")]
    public Task DeleteAsync(string id, CancellationToken cancellationToken) =>
        claims.DeleteAsync(id, cancellationToken);

    [HttpGet("{id}")]
    public Task<ClaimResponse?> GetAsync(string id, CancellationToken cancellationToken) =>
        claims.GetAsync(id, cancellationToken);
}
