namespace SportsLeague.Domain.Entities;

public class TournamentTeam : AuditBase
{
    public int TournamentId { get; set; }
    public int TeamId { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Tournament Tournament { get; set; } = null!;
    public Team Team { get; set; } = null!;
    // Partidos como local
    public ICollection<Match> HomeMatches { get; set; } = new List<Match>();
    // Partidos como visitante
    public ICollection<Match> AwayMatches { get; set; } = new List<Match>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
}