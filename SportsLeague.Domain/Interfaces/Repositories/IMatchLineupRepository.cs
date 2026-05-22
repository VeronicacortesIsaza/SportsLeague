using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Interfaces.Repositories;

public interface IMatchLineupRepository
{
    Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId);
    Task<IEnumerable<MatchLineup>> GetByMatchWithDetailsAsync(int matchId);
    Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(int matchId, int teamId);

    Task<bool> ExistsPlayerInMatchAsync(int matchId, int playerId);
    Task<int> CountStartersByMatchAndTeamAsync(int matchId, int teamId);

    Task<MatchLineup?> GetByIdAsync(int id);
    Task<MatchLineup> CreateAsync(MatchLineup matchLineup);
    Task DeleteAsync(int id);
}
