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
    public Task<IReadOnlyList<CoverResponse>> GetAll(CancellationToken cancellationToken) =>
        covers.ListAsync(cancellationToken);

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CoverResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        var cover = await covers.GetAsync(id, cancellationToken);
        return cover is null ? NotFound() : cover;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CoverResponse>> Create(CreateCoverRequest request, CancellationToken cancellationToken)
    {
        var cover = await covers.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = cover.Id }, cover);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken) =>
        await covers.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
