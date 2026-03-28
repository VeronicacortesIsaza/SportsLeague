using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SponsorController : ControllerBase
{
    private readonly ILogger<SponsorController> _logger;
    private readonly ISponsorService _sponsorService;
    private readonly IMapper _mapper;

    public SponsorController(
        ILogger<SponsorController> logger,
        ISponsorService sponsorService,
        IMapper mapper)
    {
        _logger = logger;
        _sponsorService = sponsorService;
        _mapper = mapper;
    }
    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var sponsors = await _sponsorService.GetAllAsync();
        var sponsorsDto = _mapper.Map<IEnumerable<SponsorResponseDTO>>(sponsors);
        return Ok(sponsorsDto);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var sponsor = await _sponsorService.GetByIdAsync(id);
        if (sponsor == null)
            return NotFound(new { message = $"Sponsor with ID {id} not found" });
        var sponsorDto = _mapper.Map<SponsorResponseDTO>(sponsor);
        return Ok(sponsorDto);
    }
    [HttpPost]
    public async Task<ActionResult> Create(SponsorRequestDTO dto)
    {
        try
        {
            var sponsor = _mapper.Map<Sponsor>(dto);
            var createdSponsor = await _sponsorService.CreateAsync(sponsor);
            var responseDto = _mapper.Map<SponsorResponseDTO>(createdSponsor);
            return CreatedAtAction(
                nameof(GetById),
                new { id = responseDto.Id },
                responseDto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, SponsorRequestDTO dto)
    {
        try
        {
            var sponsor = _mapper.Map<Sponsor>(dto);
            await _sponsorService.UpdateAsync(id, sponsor);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _sponsorService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    [HttpGet("{id}/tournaments")]
    public async Task<ActionResult> GetTournamentsBySponsorId(int id)
    {
        try
        {
            var tournaments = await _sponsorService.GetTournamentsBySponsorAsync(id);

            var responseDto = _mapper.Map<IEnumerable<TournamentSponsorResponseDTO>>(tournaments);

            return Ok(responseDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    [HttpPost("{id}/tournaments")]
    public async Task<ActionResult> AddSponsorToTournament(int id, TournamentSponsorRequestDTO dto)
    {
        try
        {
            var result = await _sponsorService.AddSponsorToTournamentAsync(
                id,
                dto.TournamentId,
                dto.ContractAmount
            );

            var responseDto = _mapper.Map<TournamentSponsorResponseDTO>(result);

            return Created("", responseDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    [HttpDelete("{id}/tournaments/{tid}")]
    public async Task<ActionResult> RemoveSponsorFromTournament(int id, int tid)
    {
        try
        {
            await _sponsorService.RemoveSponsorFromTournamentAsync(id, tid);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
