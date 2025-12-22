using SmartStickyReviewer.Domain.Entities;

namespace SmartStickyReviewer.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for site configuration persistence
/// </summary>
public interface ISiteConfigurationRepository
{
    Task<SiteConfiguration?> GetBySiteIdAsync(string siteId, CancellationToken cancellationToken = default);
    Task<SiteConfiguration> CreateAsync(SiteConfiguration configuration, CancellationToken cancellationToken = default);
    Task<SiteConfiguration> UpdateAsync(SiteConfiguration configuration, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string siteId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string siteId, CancellationToken cancellationToken = default);
}
