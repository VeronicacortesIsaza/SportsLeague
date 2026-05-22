using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/match/{matchId}/lineup")]
public class MatchLineupController : ControllerBase
{
    private readonly IMatchLineupService _matchLineupService;
    private readonly IMapper _mapper;

    public MatchLineupController(IMatchLineupService matchLineupService, IMapper mapper)
    {
        _matchLineupService = matchLineupService;
        _mapper = mapper;
    }

    // POST /api/match/{matchId}/lineup
    [HttpPost]
    public async Task<ActionResult<MatchLineupResponseDTO>> AddPlayer(
        int matchId,
        [FromBody] MatchLineupRequestDTO dto)
    {
        try
        {
            var lineup = _mapper.Map<MatchLineup>(dto);

            var created = await _matchLineupService.AddPlayerToLineupAsync(matchId, lineup);

            return StatusCode(201, _mapper.Map<MatchLineupResponseDTO>(created));
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

    // GET /api/match/{matchId}/lineup
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetLineup(int matchId)
    {
        try
        {
            var lineup = await _matchLineupService.GetLineupByMatchAsync(matchId);

            return Ok(_mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineup));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // GET /api/match/{matchId}/lineup/team/{teamId}
    [HttpGet("team/{teamId}")]
    public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetLineupByTeam(
        int matchId,
        int teamId)
    {
        try
        {
            var lineup = await _matchLineupService.GetLineupByMatchAndTeamAsync(matchId, teamId);

            return Ok(_mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineup));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // DELETE /api/match/{matchId}/lineup/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePlayer(int matchId, int id)
    {
        try
        {
            await _matchLineupService.DeleteLineupPlayerAsync(matchId, id);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}