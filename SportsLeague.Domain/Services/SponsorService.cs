using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using System.Text.RegularExpressions;

namespace SportsLeague.Domain.Services;

public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _sponsorRepository;
    private readonly ILogger<SponsorService> _logger;
    private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
    private readonly ITournamentRepository _tournamentRepository;

    public SponsorService(ISponsorRepository sponsorRepository, ILogger<SponsorService> logger, ITournamentSponsorRepository tournamentSponsorRepository, ITournamentRepository tournamentRepository)
    {
        _sponsorRepository = sponsorRepository;
        _logger = logger;
        _tournamentSponsorRepository = tournamentSponsorRepository;
        _tournamentRepository = tournamentRepository;
    }
    public async Task<IEnumerable<Sponsor>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all sponsors");
        return await _sponsorRepository.GetAllAsync();
    }
    public async Task<Sponsor?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);
        var sponsor = await _sponsorRepository.GetByIdAsync(id);
        if (sponsor == null)
            _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);
        return sponsor;
    }
    public async Task<Sponsor> CreateAsync(Sponsor sponsor)
    {
        // Validacion de email
        if (!Regex.IsMatch(sponsor.ContactEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new InvalidOperationException("Invalid email format");
        }
        // Validación de negocio: nombre único
        var existingSponsor = await _sponsorRepository.GetByNameAsync(sponsor.Name);
        if (existingSponsor != null)
        {
            _logger.LogWarning("Sponsor with name '{SponsorName}' already exists", sponsor.Name);
            throw new InvalidOperationException(
                $"Ya existe un patrocinador con el nombre '{sponsor.Name}'");
        }
        if (!Regex.IsMatch(sponsor.ContactEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new InvalidOperationException("Invalid email format");
        }


        _logger.LogInformation("Creating sponsor: {SponsorName}", sponsor.Name);
        return await _sponsorRepository.CreateAsync(sponsor);
    }

    public async Task UpdateAsync(int id, Sponsor sponsor)
    {
        var existingSponsor = await _sponsorRepository.GetByIdAsync(id);
        if (existingSponsor == null)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for update", id);
            throw new KeyNotFoundException(
                $"No se encontró el patrocinador con ID {id}");
        }
        if (!Regex.IsMatch(sponsor.ContactEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new InvalidOperationException("Invalid email format");
        }
        existingSponsor.Name = sponsor.Name;
        existingSponsor.WebsiteUrl = sponsor.WebsiteUrl;
        existingSponsor.ContactEmail = sponsor.ContactEmail;
        existingSponsor.Phone = sponsor.Phone;
        existingSponsor.Category = sponsor.Category;
        existingSponsor.UpdatedAt = DateTime.UtcNow;
        _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.UpdateAsync(existingSponsor);
    }
    public async Task DeleteAsync(int id)
    {
        var existingSponsor = await _sponsorRepository.GetByIdAsync(id);
        if (existingSponsor == null)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for deletion", id);
            throw new KeyNotFoundException(
                $"No se encontró el patrocinador con ID {id}");
        }
        _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.DeleteAsync(id);
    }
    public async Task<IEnumerable<TournamentSponsor>> GetTournamentsBySponsorAsync(int sponsorId)
    {
        _logger.LogInformation("Retrieving tournaments for sponsor with ID: {SponsorId}", sponsorId);

        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
        if (!sponsorExists)
            throw new KeyNotFoundException($"Sponsor with ID {sponsorId} not found");

        return await _tournamentSponsorRepository.GetBySponsorAsync(sponsorId);
    }
    public async Task<TournamentSponsor> AddSponsorToTournamentAsync(
        int sponsorId,
        int tournamentId,
        decimal contractAmount)
    {
        _logger.LogInformation(
            "Adding sponsor {SponsorId} to tournament {TournamentId}",
            sponsorId,
            tournamentId);

        if (contractAmount <= 0)
            throw new InvalidOperationException("ContractAmount must be greater than 0");

        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
        if (!sponsorExists)
            throw new KeyNotFoundException($"Sponsor with ID {sponsorId} not found");

        var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);
        if (!tournamentExists)
            throw new KeyNotFoundException($"Tournament with ID {tournamentId} not found");

        var existingRelation = await _tournamentSponsorRepository
            .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);

        if (existingRelation != null)
            throw new InvalidOperationException("Sponsor is already linked to this tournament");

        var relation = new TournamentSponsor
        {
            SponsorId = sponsorId,
            TournamentId = tournamentId,
            ContractAmount = contractAmount,
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        return await _tournamentSponsorRepository.CreateAsync(relation);
    }
    public async Task RemoveSponsorFromTournamentAsync(int sponsorId, int tournamentId)
    {
        _logger.LogInformation("Removing sponsor {SponsorId} from tournament {TournamentId}", sponsorId, tournamentId);
        var existingRelation = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
        if (existingRelation == null)
            throw new KeyNotFoundException("The sponsor is not linked to the specified tournament");
        await _tournamentSponsorRepository.DeleteAsync(existingRelation.Id);
    }
    public async Task<bool> SponsorExistsAsync(int id)
    {
        return await _sponsorRepository.ExistsAsync(id);
    }
}
