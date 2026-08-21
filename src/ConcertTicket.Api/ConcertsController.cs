using ConcertTicket.Application.Concerts.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTicket.Api;

[ApiController]
[Route("api/v1/concerts")]
public class ConcertsController : ControllerBase
{
    private readonly IConcertService _concertService;

    public ConcertsController(IConcertService concertService)
    {
        _concertService = concertService;
    }

    [HttpGet]
    public async Task<IActionResult> GetConcerts(
        CancellationToken cancellationToken)
    {
        var concerts =
            await _concertService.GetPublishedConcertsAsync(
                cancellationToken);

        return Ok(concerts);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetConcert(
        Guid id,
        CancellationToken cancellationToken)
    {
        var concert =
            await _concertService.GetConcertByIdAsync(
                id,
                cancellationToken);

        if (concert is null)
            return NotFound();

        return Ok(concert);
    }
}
