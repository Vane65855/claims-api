using Claims.Application.Contracts;
using Claims.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ClaimsController(ClaimsService claims) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ClaimResponse>> GetAll(CancellationToken cancellationToken) =>
        claims.ListAsync(cancellationToken);

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        var claim = await claims.GetAsync(id, cancellationToken);
        return claim is null ? NotFound() : claim;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClaimResponse>> Create(CreateClaimRequest request, CancellationToken cancellationToken)
    {
        var claim = await claims.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = claim.Id }, claim);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken) =>
        await claims.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
