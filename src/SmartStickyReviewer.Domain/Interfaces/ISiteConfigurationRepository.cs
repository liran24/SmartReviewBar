using SmartStickyReviewer.Domain.Entities;
using SmartStickyReviewer.Domain.ValueObjects;

namespace SmartStickyReviewer.Domain.Interfaces;

public interface ISiteConfigurationRepository
{
    Task<SiteConfiguration?> GetAsync(SiteId siteId, CancellationToken ct);
    Task UpsertAsync(SiteConfiguration configuration, CancellationToken ct);
}

