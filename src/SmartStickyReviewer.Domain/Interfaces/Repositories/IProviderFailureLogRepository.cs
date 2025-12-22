using SmartStickyReviewer.Domain.Entities;

namespace SmartStickyReviewer.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for provider failure log persistence
/// </summary>
public interface IProviderFailureLogRepository
{
    Task<ProviderFailureLog> CreateAsync(ProviderFailureLog log, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProviderFailureLog>> GetBySiteIdAsync(string siteId, int limit = 100, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProviderFailureLog>> GetUnnotifiedAsync(CancellationToken cancellationToken = default);
    Task MarkNotifiedAsync(string id, CancellationToken cancellationToken = default);
}
