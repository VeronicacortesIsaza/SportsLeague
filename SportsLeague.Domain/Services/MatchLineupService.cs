using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class MatchLineupService : IMatchLineupService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IMatchLineupRepository _matchLineupRepository;
    private readonly ILogger<MatchLineupService> _logger;

    public MatchLineupService(
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository,
        IMatchLineupRepository matchLineupRepository,
        ILogger<MatchLineupService> logger)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _matchLineupRepository = matchLineupRepository;
        _logger = logger;
    }

    public async Task<MatchLineup> AddPlayerToLineupAsync(int matchId, MatchLineup dto)
    {
        // V1: partido debe existir
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new InvalidOperationException($"No se encontró el partido con ID {matchId}");

        // V6: partido debe estar Scheduled
        if (match.Status != MatchStatus.Scheduled)
            throw new InvalidOperationException("Solo se pueden registrar alineaciones en partidos Scheduled");

        // V2: jugador debe existir
        var player = await _playerRepository.GetByIdAsync(dto.PlayerId);
        if (player == null)
            throw new InvalidOperationException($"No se encontró el jugador con ID {dto.PlayerId}");

        // V3: jugador debe pertenecer al HomeTeam o AwayTeam
        bool belongsToMatchTeams =
            player.TeamId == match.HomeTeamId ||
            player.TeamId == match.AwayTeamId;

        if (!belongsToMatchTeams)
            throw new InvalidOperationException("El jugador no pertenece a ninguno de los equipos del partido");

        // V4: no puede estar registrado dos veces en el mismo partido
        bool alreadyExists = await _matchLineupRepository.ExistsPlayerInMatchAsync(matchId, dto.PlayerId);
        if (alreadyExists)
            throw new InvalidOperationException("El jugador ya está registrado en la alineación de este partido");

        // V5: máximo 11 titulares por equipo
        if (dto.IsStarter)
        {
            int startersCount = await _matchLineupRepository.CountStartersByMatchAndTeamAsync(matchId, player.TeamId);

            if (startersCount >= 11)
                throw new InvalidOperationException("El equipo ya tiene 11 titulares registrados en este partido");
        }

        var lineup = new MatchLineup
        {
            MatchId = matchId,
            PlayerId = dto.PlayerId,
            IsStarter = dto.IsStarter,
            Position = dto.Position
        };

        var created = await _matchLineupRepository.CreateAsync(lineup);

        _logger.LogInformation("Jugador {PlayerId} agregado a alineación del partido {MatchId}", dto.PlayerId, matchId);

        return created;
    }

    public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAsync(int matchId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        return await _matchLineupRepository.GetByMatchWithDetailsAsync(matchId);
    }

    public async Task<IEnumerable<MatchLineup>> GetLineupByMatchAndTeamAsync(int matchId, int teamId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        return await _matchLineupRepository.GetByMatchAndTeamAsync(matchId, teamId);
    }

    public async Task DeleteLineupPlayerAsync(int matchId, int id)
    {
        var lineup = await _matchLineupRepository.GetByIdAsync(id);

        if (lineup == null || lineup.MatchId != matchId)
            throw new KeyNotFoundException("No se encontró el jugador en la alineación");

        await _matchLineupRepository.DeleteAsync(id);

        _logger.LogInformation("Se eliminó el registro {LineupId} del partido {MatchId}", id, matchId);
    }
}